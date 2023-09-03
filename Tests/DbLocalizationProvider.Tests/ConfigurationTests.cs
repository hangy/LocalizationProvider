using DbLocalizationProvider.Sync;
using Xunit;

namespace DbLocalizationProvider.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void UninitializedCommand_ShouldThrowNiceException()
        {
            var executor = new CommandExecutor(new ConfigurationContext().TypeFactory);
            var command = new UpdateSchema.Command();

            Assert.ThrowsAsync<HandlerNotFoundException>(async () => await executor.Execute(command));
        }
    }
}
