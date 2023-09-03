// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Refactoring;

namespace DbLocalizationProvider.Sync.Collectors
{
    internal class DisplayAttributeCollector : IResourceCollector
    {
        private readonly OldResourceKeyBuilder _oldKeyBuilder;
        private readonly DiscoveredTranslationBuilder _translationBuilder;

        public DisplayAttributeCollector(OldResourceKeyBuilder oldKeyBuilder, DiscoveredTranslationBuilder translationBuilder)
        {
            _oldKeyBuilder = oldKeyBuilder;
            _translationBuilder = translationBuilder;
        }

        public async IAsyncEnumerable<DiscoveredResource> GetDiscoveredResources(
            Type target,
            object instance,
            MemberInfo mi,
            string translation,
            string resourceKey,
            string resourceKeyPrefix,
            bool typeKeyPrefixSpecified,
            bool isHidden,
            string typeOldName,
            string typeOldNamespace,
            Type declaringType,
            Type returnType,
            bool isSimpleType)
        {
            // try to fetch also [Display()] attribute to generate new "...-Description" resource => usually used for help text labels
            var displayAttribute = mi.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute?.Description != null)
            {
                var propertyName = $"{mi.Name}-Description";
                var oldResourceKeys =
                    _oldKeyBuilder.GenerateOldResourceKey(target,
                                                          propertyName,
                                                          mi,
                                                          resourceKeyPrefix,
                                                          typeOldName,
                                                          typeOldNamespace);
                yield return new DiscoveredResource(mi,
                                                    $"{resourceKey}-Description",
                                                    await _translationBuilder.FromSingle(displayAttribute.Description),
                                                    propertyName,
                                                    declaringType,
                                                    returnType,
                                                    isSimpleType)
                {
                    TypeName = target.Name,
                    TypeNamespace = target.Namespace,
                    TypeOldName = oldResourceKeys.Item2,
                    TypeOldNamespace = typeOldNamespace,
                    OldResourceKey = oldResourceKeys.Item1
                };
            }
        }
    }
}
