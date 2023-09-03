using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Refactoring;
using DbLocalizationProvider.Sync;
using Xunit;

namespace DbLocalizationProvider.Tests.NamedResources
{
    public class NamedModelsTests
    {
        public NamedModelsTests()
        {
            var state = new ScanState();
            var ctx = new ConfigurationContext();
            var keyBuilder = new ResourceKeyBuilder(state, ctx);
            var oldKeyBuilder = new OldResourceKeyBuilder(keyBuilder);
            ctx.TypeFactory.ForQuery<DetermineDefaultCulture.Query>().SetHandler<DetermineDefaultCulture.Handler>();

            var queryExecutor = new QueryExecutor(ctx.TypeFactory);
            var translationBuilder = new DiscoveredTranslationBuilder(queryExecutor);

            _sut = new TypeDiscoveryHelper(new List<IResourceTypeScanner>
            {
                new LocalizedModelTypeScanner(keyBuilder, oldKeyBuilder, state, ctx, translationBuilder),
                new LocalizedResourceTypeScanner(keyBuilder, oldKeyBuilder, state, ctx, translationBuilder),
                new LocalizedEnumTypeScanner(keyBuilder, translationBuilder),
                new LocalizedForeignResourceTypeScanner(keyBuilder, oldKeyBuilder, state, ctx, translationBuilder)
            }, ctx);
        }

        private readonly TypeDiscoveryHelper _sut;

        [Fact]
        public async Task DuplicateAttributes_DiffProperties_SameKey_ThrowsException()
        {
            var models = new[] { typeof(BadResourceWithDuplicateKeysWithinClass) };
            var result = new List<DiscoveredResource>();

            await Assert.ThrowsAsync<DuplicateResourceKeyException>(async () =>
            {
                foreach (var model in models)
                {
                    result.AddRange(await _sut.ScanResources(model));
                }
            });
        }

        [Fact]
        public async Task DuplicateAttributes_SingleProperty_SameKey_ThrowsException()
        {
            var models = new[] { typeof(ModelWithDuplicateResourceKeys) };
            var result = new List<DiscoveredResource>();

            await Assert.ThrowsAsync<DuplicateResourceKeyException>(async () =>
            {
                foreach (var model in models)
                {
                    result.AddRange(await _sut.ScanResources(model));
                }
            });
        }

        [Fact]
        public async Task MultipleAttributeForSingleProperty_WithClassPrefix()
        {
            var models = _sut.GetTypesWithAttribute<LocalizedModelAttribute>()
                                           .Where(t => t.FullName == $"DbLocalizationProvider.Tests.NamedResources.{nameof(ModelWithNamedPropertiesWithPrefix)}");

            var properties = new List<DiscoveredResource>();
            foreach (var model in models)
            {
                properties.AddRange(await _sut.ScanResources(model));
            }

            var firstResource = properties.FirstOrDefault(p => p.Key == "/contenttypes/modelwithnamedpropertieswithprefix/resource1");

            Assert.NotNull(firstResource);
            Assert.Equal("1st resource", firstResource.Translations.DefaultTranslation());

            var secondResource = properties.FirstOrDefault(p => p.Key == "/contenttypes/modelwithnamedpropertieswithprefix/resource2");

            Assert.NotNull(secondResource);
            Assert.Equal("2nd resource", secondResource.Translations.DefaultTranslation());
        }

        [Fact]
        public async Task MultipleAttributesForSingleProperty_NoPrefix()
        {
            var models = _sut.GetTypesWithAttribute<LocalizedModelAttribute>()
                                           .Where(t => t.FullName == $"DbLocalizationProvider.Tests.NamedResources.{nameof(ModelWithNamedProperties)}");

            var properties = new List<DiscoveredResource>();
            foreach (var model in models)
            {
                properties.AddRange(await _sut.ScanResources(model));
            }

            var nonexistingProperty = properties.FirstOrDefault(p => p.Key == "DbLocalizationProvider.Tests.NamedResources.ModelWithNamedProperties.PageHeader");
            Assert.Null(nonexistingProperty);

            var namedProperty = properties.FirstOrDefault(p => p.Key == "/this/is/xpath/key");
            Assert.NotNull(namedProperty);
            Assert.Equal("This is page header", namedProperty.Translations.DefaultTranslation());

            var anotherNamedProperty = properties.FirstOrDefault(p => p.Key == "/this/is/another/xpath/key");
            Assert.NotNull(anotherNamedProperty);

            var resourceKeyOnComplexProperty = properties.FirstOrDefault(p => p.Key == "/this/is/complex/type");
            Assert.NotNull(resourceKeyOnComplexProperty);

            var propertyWithDisplayName = properties.FirstOrDefault(p => p.Key == "/simple/property/with/display/name");
            Assert.NotNull(propertyWithDisplayName);
            Assert.Equal("This is simple property", propertyWithDisplayName.Translations.DefaultTranslation());
        }

        [Fact]
        public async Task ResourceAttributeToClass_WithClassPrefix()
        {
            var models = _sut.GetTypesWithAttribute<LocalizedModelAttribute>()
                                           .Where(t => t.FullName == $"DbLocalizationProvider.Tests.NamedResources.{nameof(ModelWithNamedPropertiesWithPrefixAndKeyOnClass)}");

            var properties = new List<DiscoveredResource>();
            foreach (var model in models)
            {
                properties.AddRange(await _sut.ScanResources(model));
            }

            var firstResource = properties.FirstOrDefault(p => p.Key == "/contenttypes/modelwithnamedpropertieswithprefixandkeyonclass/name");
            Assert.NotNull(firstResource);

            var secondResource = properties.FirstOrDefault(p => p.Key == "/contenttypes/modelwithnamedpropertieswithprefixandkeyonclass/description");
            Assert.NotNull(secondResource);

            var thirdResource = properties.FirstOrDefault(p => p.Key == "/contenttypes/modelwithnamedpropertieswithprefixandkeyonclass/properties/pageheader/caption");
            Assert.NotNull(thirdResource);
        }

        [Fact]
        public async Task SingleAttributeForSingleProperty_WithClassPrefix()
        {
            var models = _sut.GetTypesWithAttribute<LocalizedModelAttribute>()
                                           .Where(t => t.FullName == $"DbLocalizationProvider.Tests.NamedResources.{nameof(ModelWithNamedPropertiesWithPrefix)}");

            var properties = new List<DiscoveredResource>();
            foreach (var model in models)
            {
                properties.AddRange(await _sut.ScanResources(model));
            }

            var name = "/contenttypes/modelwithnamedpropertieswithprefix/properties/pageheader/name";
            var headerProperty = properties.FirstOrDefault(p => p.Key == name);

            Assert.NotNull(headerProperty);
            Assert.Equal("This is page header", headerProperty.Translations.DefaultTranslation());
        }
    }
}
