// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;

namespace DbLocalizationProvider.Queries
{
    /// <summary>
    /// Which is the default language? With help of this command you can get to know this magic.
    /// </summary>
    public class DetermineDefaultCulture
    {
        /// <summary>
        /// Which is the default language? With help of this command you can get to know this magic.
        /// </summary>
        public class Query : IQuery<string> { }

        /// <summary>
        /// Default handler to answer question about which is the default language.
        /// This handler is reading <see cref="ConfigurationContext.DefaultResourceCulture" /> property.
        /// </summary>
        public class Handler : IQueryHandler<Query, string>
        {
            private const string _theDefaultCulture = "en";
            private readonly ConfigurationContext _context;

            /// <summary>
            /// Creates new instance of the handler.
            /// </summary>
            /// <param name="context">Configuration context.</param>
            public Handler(ConfigurationContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="query">This is the query instance</param>
            /// <returns>
            /// You have to return something from the query execution. Of course you can return <c>null</c> as well if you
            /// will.
            /// </returns>
            public Task<string> Execute(Query query)
            {
                return Task.FromResult(_context.DefaultResourceCulture != null
                                           ? _context.DefaultResourceCulture.Name
                                           : _theDefaultCulture);
            }
        }
    }
}
