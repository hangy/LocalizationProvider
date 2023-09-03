using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Refactoring;
using DbLocalizationProvider.Sync;
using Xunit;

namespace DbLocalizationProvider.Tests.ForeignKnownResources
{
    public class ForeignResourceScannerTests
    {
        private readonly TypeDiscoveryHelper _sut;

        public ForeignResourceScannerTests()
        {
            var state = new ScanState();
            var ctx = new ConfigurationContext();
            var keyBuilder = new ResourceKeyBuilder(state, ctx);
            var oldKeyBuilder = new OldResourceKeyBuilder(keyBuilder);
            ctx.TypeFactory.ForQuery<DetermineDefaultCulture.Query>().SetHandler<DetermineDefaultCulture.Handler>();
            ctx.ForeignResources
                .Add<ResourceWithNoAttribute>()
                .Add<BadRecursiveForeignResource>(true);

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
        public async Task DiscoverForeignResourceClass_SingleProperty()
        {
            var resources = await _sut.ScanResources(typeof(ResourceWithNoAttribute));

            Assert.True(resources.Any());

            var resource = resources.First();

            Assert.Equal("Default resource value", resource.Translations.DefaultTranslation());
            Assert.Equal("DbLocalizationProvider.Tests.ForeignKnownResources.ResourceWithNoAttribute.SampleProperty", resource.Key);
        }

        [Fact]
        public async Task DiscoverForeignResourceNestedClass()
        {
            var resources = await _sut.ScanResources(typeof(ResourceWithNoAttribute.NestedResource));

            Assert.True(resources.Any());

            var resource = resources.First();

            Assert.Equal("NestedProperty", resource.Translations.DefaultTranslation());
            Assert.Equal("DbLocalizationProvider.Tests.ForeignKnownResources.ResourceWithNoAttribute+NestedResource.NestedProperty", resource.Key);
        }

        [Fact]
        public async Task DiscoverForeignResource_Enum()
        {
            var resources = await _sut.ScanResources(typeof(SomeEnum));

            Assert.True(resources.Any());
            Assert.Equal(3, resources.Count());

            var resource = resources.First();

            Assert.Equal("None", resource.Translations.DefaultTranslation());
            Assert.Equal("DbLocalizationProvider.Tests.ForeignKnownResources.SomeEnum.None", resource.Key);
        }

        [Fact]
        public async Task ScanStackOverflowResource_WithPropertyReturningSameDeclaringType_ViaForeignResources()
        {
            var results = await _sut.ScanResources(typeof(BadRecursiveForeignResource));

            Assert.NotNull(results);
            Assert.Single(results);
        }
    }
}
