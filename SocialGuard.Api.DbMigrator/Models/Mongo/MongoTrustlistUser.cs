using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialGuard.Api.DbMigrator.Models.Mongo;

public class MongoTrustlistUser
{
	[Key, Required, BsonId, BsonRequired]
	public ulong Id { get; init; }

	[Required, BsonRequired, DisallowNull]
	public List<MongoTrustlistEntry>? Entries { get; set; }
}