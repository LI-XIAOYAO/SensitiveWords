using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace SensitiveWords
{
    /// <summary>
    /// TypeExtension
    /// </summary>
    internal static class TypeExtension
    {
        private static readonly ConcurrentDictionary<Type, (PropertyInfo IntIndex, PropertyInfo LongIndex)> _indexProps = new ConcurrentDictionary<Type, (PropertyInfo, PropertyInfo)>();

        /// <summary>
        /// IsStruct
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        /// <summary>
        /// GetIndexProperty
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static (PropertyInfo IntIndex, PropertyInfo LongIndex) GetIndexProperty(this Type type)
        {
            if (_indexProps.TryGetValue(type, out var indexProp))
            {
                return indexProp;
            }

            PropertyInfo intIndexProp = null;
            PropertyInfo longIndexProp = null;

            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanWrite || 1 != prop.GetIndexParameters().Length)
                {
                    continue;
                }

                var parameterType = prop.GetIndexParameters()[0].ParameterType;

                if (typeof(int) == parameterType)
                {
                    intIndexProp = prop;
                }
                else if (typeof(long) == parameterType)
                {
                    longIndexProp = prop;
                }

                if (null != intIndexProp && null != longIndexProp)
                {
                    break;
                }
            }

            _indexProps.TryAdd(type, indexProp);

            return (intIndexProp, longIndexProp);
        }
    }
}