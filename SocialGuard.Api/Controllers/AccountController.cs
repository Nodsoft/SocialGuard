using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialGuard.Api.Data;
using SocialGuard.Api.Data.Authentication;
using SocialGuard.Common.Data.Models.Authentication;

namespace SocialGuard.Api.Controllers;

[ApiController, Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("4.0")]
public sealed class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, AuthDbContext context)
    {
        _userManager = userManager;
        _context = context;

    }
    
    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _userManager.FindByNameAsync(model.Email) is not null)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            ApplicationUser user = new() { UserName = model.Email, Email = model.Email };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                return Ok();
            }
            
            AddErrors(result);
        }

        // If we got this far, something failed.
        return BadRequest(ModelState);
    }
    
    private void AddErrors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}