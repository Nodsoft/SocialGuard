using Microsoft.AspNetCore.Identity;

namespace SocialGuard.Api.Data.Authentication;

public class UserRole : IdentityRole<Guid>
{
	public UserRole() { }
	public UserRole(string roleName) : base(roleName) { }
		
		
	public const string Emitter = "emitter";
	public const string Admin = "admin";
	
	internal const string EmitterNormalized =  "EMITTER";
	
	internal static UserRole AdminRole { get; } = new() { Id = new("f2e50f56-bf2b-4b52-a3b6-9619350059fd"), Name = Admin, NormalizedName = Admin.ToUpperInvariant() };
	internal static UserRole EmitterRole { get; } = new() { Id = new("e386aa10-1a60-4e6b-9e86-31f0e7b4784a"), Name = Emitter, NormalizedName = Admin.ToUpperInvariant() };

}