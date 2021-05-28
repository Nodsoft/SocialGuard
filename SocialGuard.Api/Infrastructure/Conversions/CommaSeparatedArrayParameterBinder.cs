using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SocialGuard.Api.Infrastructure.Conversions
{
	public class CommaSeparatedArrayModelBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context) => CommaSeparatedArrayModelBinder.IsSupportedModelType(context.Metadata.ModelType)
			? new CommaSeparatedArrayModelBinder()
			: null;
	}


	/// <summary>
	/// Allows for integers to be passed on the URL as comma-seperated values in addition to the usual
	/// multi-named values (i.e.  param=1,2,3 instead of just param=1&amp;param=2&amp;param=3).
	/// </summary>
	public class CommaSeparatedArrayModelBinder : IModelBinder
	{
		private static readonly Type[] supportedElementTypes =
		{
			typeof(int), typeof(long), typeof(short), typeof(byte),
			typeof(uint), typeof(ulong), typeof(ushort)
		};

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (!IsSupportedModelType(bindingContext.ModelType))
			{
				bindingContext.Result = ModelBindingResult.Failed();
				return Task.CompletedTask;
			}

			ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			if (valueProviderResult.FirstValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) is string[] stringArray)
			{
				if (bindingContext.ModelType.GetElementType() is Type elementType)
				{
					bindingContext.Result = ModelBindingResult.Success(CopyAndConvertArray(stringArray, elementType));
					return Task.CompletedTask;
				}
			}

			bindingContext.Result = ModelBindingResult.Failed();
			return Task.CompletedTask;
		}

		private static Array CopyAndConvertArray(IReadOnlyList<string> sourceArray, Type elementType)
		{
			Array targetArray = Array.CreateInstance(elementType, sourceArray.Count);

			if (sourceArray.Count > 0)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(elementType);

				for (int i = 0; i < sourceArray.Count; i++)
				{
					targetArray.SetValue(converter.ConvertFromString(sourceArray[i]), i);
				}
			}

			return targetArray;
		}

		internal static bool IsSupportedModelType(Type modelType) => modelType.IsArray
			&& modelType.GetArrayRank() is 1
			&& modelType.HasElementType
			&& supportedElementTypes.Contains(modelType.GetElementType());
	}
}
