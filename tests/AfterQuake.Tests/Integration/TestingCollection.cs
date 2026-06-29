using Xunit;

namespace AfterQuake.Tests.Integration;

[CollectionDefinition("Integration Testing")]
public class TestingCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}