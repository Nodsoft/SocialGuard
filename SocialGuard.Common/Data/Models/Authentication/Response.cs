namespace SocialGuard.Common.Data.Models.Authentication;


public record Response : Response<object>
{
	public static new Response SuccessResponse() => new() { Status = "Success", Message = "Request carried out successfully." };
	public static new Response ErrorResponse() => new() { Status = "Error", Message = "Something went wrong." };
}


public record Response<T>
{
	[NotNull]
	public string Status { get; init; } = string.Empty;

	[NotNull]
	public string Message { get; init; } = string.Empty;

	[MaybeNull]
	public virtual T Details { get; init; }

	public static Response<T> SuccessResponse() => new() { Status = "Success", Message = "Request carried out successfully." };
	public static Response<T> ErrorResponse() => new() { Status = "Error", Message = "Something went wrong." };
}