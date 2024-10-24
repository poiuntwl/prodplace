namespace IntegrationTests.Factories;

[CollectionDefinition(nameof(ContainersFactoryCollectionDefinition), DisableParallelization = false)]
public class ContainersFactoryCollectionDefinition : ICollectionFixture<ContainersFactory>;