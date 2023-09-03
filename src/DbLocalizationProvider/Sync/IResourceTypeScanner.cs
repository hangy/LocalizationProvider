// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;

namespace DbLocalizationProvider.Sync
{
    public interface IResourceTypeScanner
    {
        bool ShouldScan(Type target);

        string GetResourceKeyPrefix(Type target, string keyPrefix = null);

        Task<ICollection<DiscoveredResource>> GetClassLevelResources(Type target, string resourceKeyPrefix);

        Task<ICollection<DiscoveredResource>> GetResources(Type target, string resourceKeyPrefix);
    }
}
