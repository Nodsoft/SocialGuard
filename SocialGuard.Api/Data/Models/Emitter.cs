using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;



namespace SocialGuard.Api.Data.Models
{

	/// <summary>
	/// Represents an Emitter profile.
	/// </summary>
	public record Emitter
	{
		[BsonId, BsonRepresentation(BsonType.String)]
		public string Login { get; init; }

		[Required]
		public EmitterType EmitterType { get; init; }

		public ulong Snowflake { get; init; }

		[Required]
		public string DisplayName { get; init; }
	}

	/// <summary>
	/// Represents types of Emitter profiles.
	/// </summary>
	public enum EmitterType : byte
	{
		/// <summary>
		/// Represents an unknown, undefined, or other Emitter profile type.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Emitter is a singular user.
		/// </summary>
		User = 1,

		/// <summary>
		/// Emitter is a Discord server.
		/// </summary>
		Server = 2
	}
}
