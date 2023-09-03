// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Internal;

namespace DbLocalizationProvider.Sync
{
    internal class LocalizedEnumTypeScanner : IResourceTypeScanner
    {
        private readonly ResourceKeyBuilder _keyBuilder;
        private readonly DiscoveredTranslationBuilder _translationBuilder;

        public LocalizedEnumTypeScanner(ResourceKeyBuilder keyBuilder, DiscoveredTranslationBuilder translationBuilder)
        {
            _keyBuilder = keyBuilder;
            _translationBuilder = translationBuilder;
        }

        public bool ShouldScan(Type target)
        {
            return target.BaseType == typeof(Enum) && target.GetCustomAttribute<LocalizedResourceAttribute>() != null;
        }

        public string GetResourceKeyPrefix(Type target, string keyPrefix = null)
        {
            var resourceAttribute = target.GetCustomAttribute<LocalizedResourceAttribute>();

            return !string.IsNullOrEmpty(resourceAttribute?.KeyPrefix)
                ? resourceAttribute.KeyPrefix
                : string.IsNullOrEmpty(keyPrefix)
                    ? target.FullName
                    : keyPrefix;
        }

        public Task<ICollection<DiscoveredResource>> GetClassLevelResources(Type target, string resourceKeyPrefix)
        {
            return Task.FromResult((ICollection<DiscoveredResource>)Enumerable.Empty<DiscoveredResource>().ToList());
        }

        public async Task<ICollection<DiscoveredResource>> GetResources(Type target, string resourceKeyPrefix)
        {
            var enumType = Enum.GetUnderlyingType(target);
            var isHidden = target.GetCustomAttribute<HiddenAttribute>() != null;

            string GetEnumTranslation(MemberInfo mi)
            {
                var result = mi.Name;
                var displayAttribute = mi.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    result = displayAttribute.Name;
                }

                return result;
            }

            var result = new List<DiscoveredResource>();

            foreach (var mi in target.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                var isResourceHidden = isHidden || mi.GetCustomAttribute<HiddenAttribute>() != null;
                var resourceKey = _keyBuilder.BuildResourceKey(target, mi.Name);
                var translations = await _translationBuilder.GetAllTranslations(mi, resourceKey, GetEnumTranslation(mi));

                result.Add(new DiscoveredResource(
                               mi,
                               resourceKey,
                               translations,
                               mi.Name,
                               target,
                               enumType,
                               enumType.IsSimpleType(),
                               isResourceHidden));
            }

            return result;
        }
    }
}
