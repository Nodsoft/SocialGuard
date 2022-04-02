using AspNetCore.Identity.Mongo.Model;
using Mapster;
using MongoDB.Bson;
using SocialGuard.Api.DbMigrator.Models.Mongo;
using SocialGuard.Api.Services.Authentication;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.DbMigrator;

public static class Mappings
{
	public static void ConfigureMapper()
	{
		TypeAdapterConfig<MongoTrustlistEntry, TrustlistEntry>
			.NewConfig()
			.Map(dest => dest.Id, src => Guid.NewGuid());

		TypeAdapterConfig<MongoUser<string>, ApplicationUser>
			.NewConfig()
			.Map(dest => dest.Id, _ => Guid.NewGuid());
		
		TypeAdapterConfig<MongoRole<string>, UserRole>
			.NewConfig()
			.Map(dest => dest.Id, _ => Guid.NewGuid());
	}
	
	internal static Guid ToGuid(this ObjectId oid)
	{            
		byte[] bytes = oid.ToByteArray().Concat(new byte[] { 0, 0, 0, 0 }).ToArray();
		return new(bytes);
	}
}