namespace Transcom.SocialGuard.Api.Services.Authentication.Models
{
	public record Response
	{
		public string Status { get; init; }
		public string Message { get; init; }
		public object Details { get; init; }

		public static Response SuccessResponse() => new() { Status = "Success", Message = "Request carried out successfully." };
		public static Response ErrorResponse() => new() { Status = "Error", Message = "Something went wrong." };
	}
}
