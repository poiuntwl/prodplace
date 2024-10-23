using System.Net.Mime;
using System.Text;
using System.Text.Json;
using CommonModels.OutboxModels;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using MassTransit.Testing;
using MessagingTools.Contracts;
using Microsoft.Extensions.DependencyInjection;
using UserService.Consumers;
using UserService.Data;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class RegisterTests :
    IClassFixture<IdentityServiceFactory>,
    IClassFixture<CustomerServiceFactory>,
    IAsyncLifetime
{
    private readonly IIdentityServiceHttpClient _identityHttpClient;
    private readonly IServiceScope _customerServiceScope;
    private readonly IServiceProvider _identityServiceScope;
    private readonly ITestHarness _testHarness;

    public RegisterTests(IdentityServiceFactory identityServiceFactory, CustomerServiceFactory customerServiceFactory)
    {
        _identityHttpClient = identityServiceFactory.HttpClient;
        _customerServiceScope = customerServiceFactory.ServiceScope;
        _identityServiceScope = identityServiceFactory.ServiceProvider;
        _testHarness = _customerServiceScope.ServiceProvider.GetTestHarness();
    }

    public async Task InitializeAsync()
    {
        await _testHarness.Start();
    }

    public Task DisposeAsync()
    {
        _customerServiceScope.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ShouldCreateCustomer()
    {
        var registerDto = new RegisterDto
        {
            Username = "someusername",
            Email = "someusername@gmail.com",
            Password = "Somevalidpassword1!"
        };

        var response = await _identityHttpClient.Register(registerDto);
        response.Should().NotBeNull();
        response!.Email.Should().Be(registerDto.Email);

        await WaitUntilAllMessagesProcessedAsync();

        var consumerTestHarness = _testHarness.GetConsumerHarness<OutboxMessagePostedConsumer>();
        var anyMessages = await consumerTestHarness.Consumed.Any<OutboxMessagePostedEvent>();
        anyMessages.Should().BeTrue();

        var customerDbContext = _customerServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customers = customerDbContext.Customers.ToList();
        customers.Should().ContainSingle(x => x.Email == registerDto.Email);
    }

    private static UserCreatedEventData? DeserializeContent(IReceivedMessage<OutboxMessagePostedEvent> x)
    {
        return JsonSerializer.Deserialize<UserCreatedEventData>(x.Context.Message.OutboxMessage.Content);
    }

    private async Task WaitUntilAllMessagesProcessedAsync()
    {
        var dbContext = _identityServiceScope.GetRequiredService<IdentityService.Data.AppDbContext>();
        while (dbContext.OutboxMessages.Any(x => x.ProcessedAt == null))
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}