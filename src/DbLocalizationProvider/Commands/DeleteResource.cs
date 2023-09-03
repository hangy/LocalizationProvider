// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Cache;

namespace DbLocalizationProvider.Commands
{
    /// <summary>
    /// Deletes single resource.
    /// </summary>
    public class DeleteResource
    {
        /// <summary>
        /// Removes single resource
        /// </summary>
        public class Handler : ICommandHandler<Command>
        {
            private readonly ConfigurationContext _configurationContext;
            private readonly IResourceRepository _repository;

            /// <summary>
            /// Creates new instance of the class.
            /// </summary>
            /// <param name="configurationContext">Configuration settings.</param>
            /// <param name="repository">Resource repository</param>
            public Handler(ConfigurationContext configurationContext, IResourceRepository repository)
            {
                _configurationContext = configurationContext;
                _repository = repository;
            }

            /// <summary>
            /// Handles the command. Actual instance of the command being executed is passed-in as argument
            /// </summary>
            /// <param name="command">Actual command instance being executed</param>
            /// <exception cref="ArgumentNullException">Key</exception>
            /// <exception cref="InvalidOperationException">Cannot delete resource `{command.Key}` that is synced with code</exception>
            public async Task Execute(Command command)
            {
                if (string.IsNullOrEmpty(command.Key))
                {
                    throw new ArgumentNullException(nameof(command), "Key is empty");
                }

                var resource = await _repository.GetByKeyAsync(command.Key);

                if (resource == null)
                {
                    return;
                }

                if (resource.FromCode)
                {
                    throw new InvalidOperationException($"Cannot delete resource `{command.Key}` that is synced with code");
                }

                await _repository.DeleteResourceAsync(resource);

                _configurationContext.CacheManager.Remove(CacheKeyHelper.BuildKey(command.Key));
            }
        }

        /// <summary>
        /// Execute this command if you need to just delete single resource.
        /// </summary>
        /// <seealso cref="DbLocalizationProvider.Abstractions.ICommand" />
        public class Command : ICommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Command" /> class.
            /// </summary>
            /// <param name="key">The key.</param>
            public Command(string key)
            {
                Key = key;
            }

            /// <summary>
            /// Gets the key for the resource to delete.
            /// </summary>
            public string Key { get; }
        }
    }
}
