namespace IntegrationTests.Factories;

[CollectionDefinition(nameof(ContainersFactoryCollectionDefinition))]
public class ContainersFactoryCollectionDefinition : ICollectionFixture<ContainersFactory>;

[CollectionDefinition(nameof(IntegrationCollection))]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>;