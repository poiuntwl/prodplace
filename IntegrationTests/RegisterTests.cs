using AuthTools.Constants;
using FluentAssertions;
using IdentityService.Dtos;
using IdentityService.Services;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using Keycloak.Net;
using MassTransit.Testing;
using MessagingTools.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService.Consumers;
using UserService.Data;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class RegisterTests :
    IClassFixture<IdentityServiceFactory>,
    IClassFixture<CustomerServiceFactory>,
    IAsyncLifetime
{
    private readonly IServiceProvider _customerServiceProvider;
    private readonly IIdentityServiceHttpClient _identityHttpClient;
    private readonly IServiceProvider _identityServiceScope;
    private readonly ITestHarness _testHarness;
    private readonly KeycloakClient _keycloakClient;

    public RegisterTests(IdentityServiceFactory identityServiceFactory, CustomerServiceFactory customerServiceFactory)
    {
        _identityHttpClient = identityServiceFactory.HttpClient;
        _customerServiceProvider = customerServiceFactory.ServiceProvider;
        _identityServiceScope = identityServiceFactory.ServiceProvider;
        _keycloakClient = identityServiceFactory.KeycloakClient;
        _testHarness = _customerServiceProvider.GetTestHarness();
    }

    public async Task InitializeAsync()
    {
        await _testHarness.Start();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ShouldCreateCustomer()
    {
        var registerDto = await RegisterUserAsync();

        await WaitUntilAllMessagesProcessedAsync();

        var consumerTestHarness = _testHarness.GetConsumerHarness<OutboxMessagePostedConsumer>();
        var anyMessages = await consumerTestHarness.Consumed.Any<OutboxMessagePostedEvent>();
        anyMessages.Should().BeTrue();

        var customerDbContext = _customerServiceProvider.GetRequiredService<AppDbContext>();
        var customers = customerDbContext.Customers.ToList();
        customers.Should().ContainSingle(x => x.Email == registerDto.Email);
    }

    [Fact]
    public async Task AssignRole_MustHaveRole()
    {
        var user = await RegisterUserAsync();

        await WaitUntilAllMessagesProcessedAsync();
        var keycloakService = _identityServiceScope.GetRequiredService<IKeycloakService>();

        var users = await _keycloakClient.GetUsersAsync("master");
        var userId = users.SingleOrDefault(x => x.Email == user.Email)?.Id;
        userId.Should().NotBeNull();

        await keycloakService.AssignRoleAsync(userId, RoleNames.Manager, CancellationToken.None);

        var roles = await _keycloakClient.GetRoleMappingsForUserAsync("master", userId, CancellationToken.None);
        var mappedRoleNames = roles.RealmMappings.Select(x => x.Name);
        mappedRoleNames.Should().Contain(RoleNames.Manager);
    }

    private async Task<UserDataResult> RegisterUserAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = $"{Guid.NewGuid().ToString()[..5]}@gmail.com",
            Password = $"Some{Guid.NewGuid().ToString()[..5]}password1!"
        };

        var response = await _identityHttpClient.Register(registerDto);
        response.Should().NotBeNull();
        response!.Email.Should().Be(registerDto.Email);
        return response;
    }

    private async Task WaitUntilAllMessagesProcessedAsync()
    {
        var counter = 3;
        var dbContext = _identityServiceScope.GetRequiredService<IdentityService.Data.AppDbContext>();
        while (counter-- > 0 && dbContext.OutboxMessages.AsNoTracking().Any(x => x.ProcessedAt == null))
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
    }
}