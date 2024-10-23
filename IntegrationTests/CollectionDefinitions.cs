using IntegrationTests.Factories;

namespace IntegrationTests;

[CollectionDefinition(nameof(ContainersFactoryCollectionDefinition))]
public class ContainersFactoryCollectionDefinition : ICollectionFixture<ContainersFactory>;

[CollectionDefinition(nameof(IdentityServiceCollectionDefinition))]
public class IdentityServiceCollectionDefinition : ICollectionFixture<IdentityServiceFactory>;

[CollectionDefinition(nameof(CustomerServiceCollectionDefinition))]
public class CustomerServiceCollectionDefinition : ICollectionFixture<CustomerServiceFactory>;