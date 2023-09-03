using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Queries;
using DbLocalizationProvider.Refactoring;
using DbLocalizationProvider.Sync;
using Xunit;

namespace DbLocalizationProvider.Tests.ScalarCollectionTests
{
    public class ScalarCollectionTests
    {
        private readonly TypeDiscoveryHelper _sut;

        public ScalarCollectionTests()
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
        public async Task ScanResourceWillScalarEnumerables_ShouldDiscover()
        {
            var properties = await _sut.ScanResources(typeof(ResourceClassWithScalarCollection));

            Assert.Equal(2, properties.Count());
        }

        [Fact]
        public async Task ScanModelWillScalarEnumerables_ShouldDiscover()
        {
            var properties = await _sut.ScanResources(typeof(ModelClassWithScalarCollection));

            Assert.Equal(2, properties.Count());
        }
    }

    [LocalizedResource]
    public class ResourceClassWithScalarCollection
    {
        public int[] ArrayOfItns { get; set; }

        public List<string> CollectionOfStrings { get; set; }
    }

    [LocalizedModel]
    public class ModelClassWithScalarCollection
    {
        public int[] ArrayOfItns { get; set; }

        public List<string> CollectionOfStrings { get; set; }
    }
}
