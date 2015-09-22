using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SoundBoard.AutoUpdate.Helpers
{
    internal static class AttributeHelper
    {
        private static Dictionary<Type, Func<string, object>> converters = new Dictionary<Type, Func<string, object>>()
        {
            {typeof(Version), ConvertVersion},
            {typeof(Uri), ConvertUri}
        };

        public static T GetAttributeByKeyOrNull<T>(this XmlAttributeCollection attributes, string key) where T : class
        {
            var attributeValue = default(T);

            var attributeQuery = attributes.AsEnumerable<XmlAttribute>();
            var attribute = attributeQuery.FirstOrDefault(w => w.Name == key);

            if (attribute != null && attribute.Value != null)
            {
                // Try use a known converter first.
                // else try casting using built-in conversion.

                Func<string, object> conversionFunction;
                if (converters.TryGetValue(typeof(T), out conversionFunction))
                    return conversionFunction(attribute.Value) as T;
                else
                    return attribute.Value as T;

            }

            return attributeValue;
        }

        public static T GetAttributeByKey<T>(this XmlAttributeCollection attributes, string key)
        {
            return (T)(object)attributes[key].Value;
        }

        private static IEnumerable<T> AsEnumerable<T>(this ICollection collection)
        {
            foreach (var item in collection)
                yield return (T)item;
        }

        private static Version ConvertVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return null;

            return new Version(version);
        }

        private static Uri ConvertUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            return new Uri(uri);
        }

    }
}
