﻿using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SocialGuard.Api.Data.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;


namespace SocialGuard.Api.Controllers;
#nullable enable

[ApiController, Route("api/v{version:apiVersion}/")]
[ApiVersion("4.0")]
public sealed class AuthorizationController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }
    
    /// <summary>
    /// Exchanges the user credentials for an access token.
    /// </summary>
    /// <returns>The response that contains the access token.</returns>
    /// <exception cref="InvalidOperationException">The OpenID Connect request cannot be retrieved.</exception>
    /// <exception cref="InvalidOperationException">The specified grant type is not implemented.</exception>
    /// <response code="200" type="application/json">The response that contains the access token.</response>
    /// <response code="400" type="application/json">The response that contains the error details.</response>
    [HttpPost("connect/token"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() 
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        if (request.IsPasswordGrantType())
        {
            ApplicationUser user = await _userManager.FindByNameAsync(request.Username!);
            if (user is null)
            {

                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    { OpenIddictServerAspNetCoreConstants.Properties.Error, Errors.InvalidGrant },
                    { OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription, "The username/password couple is invalid." }
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Validate the username/password parameters and ensure the account is not locked out.
            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {

                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    { OpenIddictServerAspNetCoreConstants.Properties.Error, Errors.InvalidGrant },
                    { OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription, "The username/password couple is invalid." }
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            ClaimsIdentity identity = new(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                    .SetClaims(Claims.Role, [..await _userManager.GetRolesAsync(user)]);

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(new(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the refresh token.
            ApplicationUser user = await _userManager.FindByIdAsync(result.Principal.GetClaim(Claims.Subject));
            if (user == null)
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            ClaimsIdentity identity = new(result.Principal.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Override the user claims present in the principal in case they changed since the refresh token was issued.
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                .SetClaims(Claims.Role, [..await _userManager.GetRolesAsync(user)]);

            identity.SetDestinations(GetDestinations);

            return SignIn(new(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not implemented.");
    }
    
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
            {
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;
            }

            case Claims.Email:
            {
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;
            }

            case Claims.Role:
            {
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;
            }

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp":
            {
                yield break;
            }

            default:
            {
                yield return Destinations.AccessToken;
                yield break;
            }
        }
    }
}