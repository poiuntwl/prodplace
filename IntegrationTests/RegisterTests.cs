using System.Net.Mime;
using System.Text;
using System.Text.Json;
using CommonModels.OutboxModels;
using FluentAssertions;
using IdentityService.Dtos;
using MassTransit.Testing;
using MessagingTools.Contracts;
using Microsoft.Extensions.DependencyInjection;
using UserService.Consumers;
using UserService.Data;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class RegisterTests : IClassFixture<IdentityServiceFactory>, IClassFixture<CustomerServiceFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _identityHttpClient;
    private readonly IServiceScope _customerServiceScope;
    private readonly IServiceScope _identityServiceScope;
    // private readonly ITestHarness _testHarness;

    public RegisterTests(IdentityServiceFactory identityServiceFactory,
        CustomerServiceFactory customerServiceFactory)
    {
        _identityHttpClient = identityServiceFactory.HttpClient;
        _customerServiceScope = customerServiceFactory.Services.CreateScope();
        _identityServiceScope = identityServiceFactory.Services.CreateScope();
        // _testHarness = _serviceScope.ServiceProvider.GetRequiredService<ITestHarness>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _customerServiceScope.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ShouldCreateCustomer()
    {
        var testHarness = _customerServiceScope.ServiceProvider.GetTestHarness();
        await testHarness.Start();

        var registerDto = new RegisterDto
        {
            Username = "someusername",
            Email = "someusername@gmail.com",
            Password = "Somevalidpassword1!"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/register");
        request.Content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8,
            MediaTypeNames.Application.Json);

        using var response = await _identityHttpClient.SendAsync(request);
        UserDataResult userDataResult;
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            userDataResult = JsonSerializer.Deserialize<UserDataResult>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        userDataResult.Should().NotBeNull();
        userDataResult!.Email.Should().Be(registerDto.Email);

        await WaitUntilAllMessagesProcessedAsync();

        var consumerTestHarness = testHarness.GetConsumerHarness<OutboxMessagePostedConsumer>();
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
        var dbContext = _identityServiceScope.ServiceProvider.GetRequiredService<IdentityService.Data.AppDbContext>();
        while (dbContext.OutboxMessages.Any(x => x.ProcessedAt == null))
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}