using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Refactoring;
using DbLocalizationProvider.Sync;
using Xunit;

namespace DbLocalizationProvider.Tests.HiddenResourcesTests
{
    public class HiddenResourceTests
    {
        private readonly TypeDiscoveryHelper _sut;

        public HiddenResourceTests()
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

        [Fact]
        public async Task DiscoverHiddenEnumProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeEnumWithHiddenResources));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.False(firstResource.IsHidden);

            var middleResource = result.First(r => r.Key == "DbLocalizationProvider.Tests.HiddenResourcesTests.SomeEnumWithHiddenResources.Some");
            Assert.True(middleResource.IsHidden);
        }

        [Fact]
        public async Task DiscoverHiddenEnumProperties_WithHiddenAttributeOnClassProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeEnumWithAllHiddenResources));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.True(firstResource.IsHidden);

            var middleResource = result.First(r => r.Key == "DbLocalizationProvider.Tests.HiddenResourcesTests.SomeEnumWithAllHiddenResources.Some");
            Assert.True(middleResource.IsHidden);
        }

        [Fact]
        public async Task DiscoverHiddenModelProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeModelWithHiddenProperty));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.True(firstResource.IsHidden);
        }

        [Fact]
        public async Task DiscoverHiddenModelProperties_WithHiddenAttributeOnClassProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeModelWithHiddenPropertyOnClassLevel));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.True(firstResource.IsHidden);
        }

        [Fact]
        public async Task DiscoverHiddenResourceProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeResourcesWithHiddenProperties));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.True(firstResource.IsHidden);

            var secondResource = result.First(r => r.Key == "DbLocalizationProvider.Tests.HiddenResourcesTests.SomeResourcesWithHiddenProperties.AnotherProperty");
            Assert.False(secondResource.IsHidden);
        }

        [Fact]
        public async Task DiscoverHiddenResources_WithHiddenAttributeOnClassProperties()
        {
            var result = await _sut.ScanResources(typeof(SomeResourcesWithHiddenOnClassLevel));

            Assert.NotEmpty(result);
            var firstResource = result.First();
            Assert.True(firstResource.IsHidden);
        }
    }
}
