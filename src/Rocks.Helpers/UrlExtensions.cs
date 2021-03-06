﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FastMember;
using JetBrains.Annotations;
#if NET471
using System.Web.Routing;

#else
    using Microsoft.AspNetCore.Routing;
#endif

namespace Rocks.Helpers
{
    public static class UrlExtensions
    {
        private static readonly ConcurrentDictionary<string, TypeInfo> typesInfoCache =
            new ConcurrentDictionary<string, TypeInfo>(StringComparer.Ordinal);


        /// <summary>
        ///     Creates new <see cref="RouteValueDictionary" /> from <paramref name="obj" /> properties.
        ///     Optionally filters property names by <paramref name="filter" />.
        /// </summary>
        /// <param name="obj">Object which properties will be added to <see cref="RouteValueDictionary" />. Can be null.</param>
        /// <param name="filter">
        ///     Optional filter function that should return true for every property that should be included in the result.
        /// </param>
        [NotNull]
        public static RouteValueDictionary PropertiesToRouteValueDictionary([CanBeNull] this object obj, Func<PropertyInfo, bool> filter = null)
        {
            var res = new RouteValueDictionary();

            if (obj == null)
                return res;

            var object_type = obj.GetType();
            var type_accessor = TypeAccessor.Create(object_type);
            var type_info = GetTypeInfo(object_type);

            foreach (var member in type_accessor.GetMembers())
            {
                if (!type_info.Properties.TryGetValue(member.Name, out var property))
                    continue;

                if (filter != null && !filter(property))
                    continue;

                var value = type_accessor[obj, member.Name];

                if (type_info.PropertiesCustomFormat.TryGetValue(member.Name, out var display_format_attribute))
                    value = FormatValue(value, display_format_attribute);

                res.Add(member.Name, value);
            }

            return res;
        }


        /// <summary>
        ///     Creates HTTP GET query parameters string from the supplied <paramref name="obj" /> properties.
        ///     Optionally filters property names by <paramref name="filter" />.
        ///     Adds specified <paramref name="prefix" /> to the start of the result string if there are any
        ///     parameters in the resulting query string.
        /// </summary>
        /// <param name="obj">Object which properties will be the result query string. Can be null.</param>
        /// <param name="prefix">
        ///     Prefix that will be added to the to the start of the result string if there are any
        ///     parameters in the resulting query string.
        /// </param>
        /// <param name="filter">
        ///     Optional filter function that should return true for every property that should be included in the result.
        /// </param>
        public static string PropertiesToQueryParameters(this object obj, string prefix = "?", Func<PropertyInfo, bool> filter = null)
        {
            if (obj == null)
                return string.Empty;

            return obj.PropertiesToRouteValueDictionary(filter).ToQueryStringParameters(prefix);
        }


        /// <summary>
        ///     Creates HTTP GET query parameters string from the supplied <paramref name="values" />.
        ///     Adds specified <paramref name="prefix" /> to the start of the result string if there are any
        ///     parameters in the resulting query string.
        /// </summary>
        /// <param name="values">A list of values that will be added to the result query string.</param>
        /// <param name="prefix">
        ///     Prefix that will be added to the to the start of the result string if there are any
        ///     parameters in the resulting query string.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static string ToQueryStringParameters(this IEnumerable<KeyValuePair<string, object>> values, string prefix = "?")
        {
            if (values == null)
                return string.Empty;

            var result = new StringBuilder(250);

            foreach (var kv in values.Where(x => x.Value != null))
            {
                if (kv.Value is string)
                {
                    AppendQueryStringValue(result, kv.Key, kv.Value);
                    continue;
                }

                if (kv.Value is IEnumerable e)
                {
                    var enm = e.GetEnumerator();
                    while (enm.MoveNext())
                        AppendQueryStringValue(result, kv.Key, enm.Current);

                    continue;
                }

                AppendQueryStringValue(result, kv.Key, kv.Value);
            }

            if (result.Length == 0)
                return string.Empty;

            return prefix + result;
        }


        private static object FormatValue(object value, [NotNull] DisplayFormatAttribute displayFormatAttribute)
        {
            if (value == null)
                return displayFormatAttribute.NullDisplayText;

            if (string.IsNullOrEmpty(displayFormatAttribute.DataFormatString))
                return value;

            return string.Format(CultureInfo.InvariantCulture, displayFormatAttribute.DataFormatString, value);
        }


        private static TypeInfo GetTypeInfo(Type type)
        {
            return typesInfoCache.GetOrAdd(type.FullName,
                                           _ =>
                                           {
                                               var type_info =
                                                   new TypeInfo
                                                   {
                                                       PropertiesCustomFormat = new Dictionary<string, DisplayFormatAttribute>(),
                                                       Properties = new Dictionary<string, PropertyInfo>()
                                                   };

                                               foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                               {
                                                   type_info.Properties[property.Name] = property;

                                                   var attr = property.GetCustomAttribute<DisplayFormatAttribute>(false);
                                                   if (attr == null)
                                                       continue;

                                                   type_info.PropertiesCustomFormat[property.Name] = attr;
                                               }

                                               return type_info;
                                           });
        }


        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void AppendQueryStringValue([NotNull] StringBuilder result,
                                                   [NotNull] string key,
                                                   [CanBeNull] object value,
                                                   bool escapeValue = true)
        {
            if (value == null)
                return;
            
            var str = value is string s ? s : Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(str))
                return;

            if (result.Length > 0)
                result.Append('&');

            result.Append(char.ToLower(key[0], CultureInfo.InvariantCulture));
            result.Append(key, 1, key.Length - 1);
            result.Append('=');
            result.Append(escapeValue ? Uri.EscapeDataString(str) : str);
        }


        private class TypeInfo
        {
            public IDictionary<string, DisplayFormatAttribute> PropertiesCustomFormat { get; set; }
            public IDictionary<string, PropertyInfo> Properties { get; set; }
        }
    }
}