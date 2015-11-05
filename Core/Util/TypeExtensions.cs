using System;
using System.Collections.Generic;
using System.Linq;

namespace EnergonSoftware.Core.Util
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this Type type)
        {
            return typeof(short) == type || typeof(int) == type || typeof(long) == type
                || typeof(float) == type || typeof(double) == type;
        }

        public static bool HasProperties(this Type type)
        {
            return type.GetProperties().Length > 0;
        }

        public static bool IsGenericCollection(this Type type)
        {
            return type.IsGenericType
                && ((typeof(ICollection<>) == type.GetGenericTypeDefinition() || typeof(IReadOnlyCollection<>) == type.GetGenericTypeDefinition())
                    || type.GetInterfaces().Any(
                        x => x.IsGenericType && (typeof(ICollection<>) == x.GetGenericTypeDefinition() || typeof(IReadOnlyCollection<>) == x.GetGenericTypeDefinition())));
        }

        public static bool IsGenericDictionary(this Type type)
        {
            return type.IsGenericType
                && ((typeof(IDictionary<,>) == type.GetGenericTypeDefinition() || typeof(IReadOnlyDictionary<,>) == type.GetGenericTypeDefinition())
                    || type.GetInterfaces().Any(
                        x => x.IsGenericType
                            && (typeof(IDictionary<,>) == x.GetGenericTypeDefinition() || typeof(IReadOnlyDictionary<,>) == x.GetGenericTypeDefinition())));
        }
    }
}
