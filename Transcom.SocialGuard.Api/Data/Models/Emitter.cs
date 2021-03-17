using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;



namespace Transcom.SocialGuard.Api.Data.Models
{
	public record Emitter
	{
		[BsonId]
		public string Login { get; init; }

		[Required]
		public EmitterType EmitterType { get; init; }

		public ulong Snowflake { get; init; }

		[Required]
		public string DisplayName { get; init; }
	}

	public enum EmitterType
	{ 
		Unknown,
		User,
		Server
	}
}
