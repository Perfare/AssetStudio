#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
using System;
using System.Linq;
using System.Reflection;

namespace SpirV
{
	public static class EnumValuesExtensions
	{
		public static Array GetEnumValues(this System.Type _this)
		{
			TypeInfo typeInfo = _this.GetTypeInfo ();
			if (!typeInfo.IsEnum) {
				throw new ArgumentException ("GetEnumValues: Type '" + _this.Name + "' is not an enum");
			}

			return
				(
				  from field in typeInfo.DeclaredFields
				  where field.IsLiteral
				  select field.GetValue (null)
				)
				.ToArray();
		}

		public static string GetEnumName(this System.Type _this, object value)
		{
			TypeInfo typeInfo = _this.GetTypeInfo ();
			if (!typeInfo.IsEnum) {
				throw new ArgumentException ("GetEnumName: Type '" + _this.Name + "' is not an enum");
			}
			return
				(
				  from field in typeInfo.DeclaredFields
				  where field.IsLiteral && (uint)field.GetValue(null) == (uint)value
				  select field.Name
				)
				.First();
		}
	}
}
#endif