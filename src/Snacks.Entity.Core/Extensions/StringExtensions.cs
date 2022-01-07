using System;
using System.Reflection;

namespace Snacks.Entity.Core
{
    internal static class StringExtensions
    {
        public static object ConvertToPropertyType(this string value, PropertyInfo property)
        {
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType == typeof(DateTime))
            {
                return DateTime.Parse(value);
            }
            else if (propertyType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else
            {
                return Convert.ChangeType(value, propertyType);
            }
        }
    }
}