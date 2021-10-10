namespace SocialGuard.Common.Data.Models.Authentication;


public record Response
{
	[NotNull]
	public string Status { get; init; } = string.Empty;

	[NotNull]
	public string Message { get; init; } = string.Empty;

	[MaybeNull]
	public object Details { get; init; }

	public static Response SuccessResponse() => new() { Status = "Success", Message = "Request carried out successfully." };
	public static Response ErrorResponse() => new() { Status = "Error", Message = "Something went wrong." };
}
