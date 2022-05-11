using System.ComponentModel;



namespace SocialGuard.Api.Infrastructure.Conversions
{
	public class UlongStringArrayConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}
}
