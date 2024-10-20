using System.Net.Mime;
using System.Text;
using FluentAssertions;
using IdentityService.Dtos;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;
using UserService.Data;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class RegisterTests : IClassFixture<IdentityServiceFactory>, IClassFixture<CustomerServiceFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _identityHttpClient;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly IServiceScope _serviceScope;

    public RegisterTests(ContainersFactory containersFactory, IdentityServiceFactory identityServiceFactory,
        CustomerServiceFactory customerServiceFactory)
    {
        _identityHttpClient = identityServiceFactory.HttpClient;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
        _serviceScope = customerServiceFactory.Services.CreateScope();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _serviceScope.Dispose();
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/register");
        request.Content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8,
            MediaTypeNames.Application.Json);

        using var response = await _identityHttpClient.SendAsync(request);
        UserDataResult userDataResult;
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            userDataResult = JsonSerializer.Deserialize<UserDataResult>(responseJson);
        }

        await WaitForEmptyQueueAsync(_rabbitMqContainer.GetConnectionString(), "rabbitmq");

        var dbContext = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customers = dbContext.Customers.ToList();
        customers.Should().ContainSingle(x => x.Email == registerDto.Email);
    }

    private static async Task WaitForEmptyQueueAsync(string connectionString, string queueName)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        Console.WriteLine("Connected to RabbitMQ");

        var retries = 0;
        while (true)
        {
            var queueDeclareOk = channel.QueueDeclarePassive(queueName);
            var messageCount = queueDeclareOk.MessageCount;

            if (messageCount == 0 && retries++ == 3)
            {
                Console.WriteLine("No messages left in the queue.");
                break;
            }

            Console.WriteLine($"Messages remaining in queue: {messageCount}");
            await Task.Delay(TimeSpan.FromSeconds(1)); // Wait for 1 second before checking again
        }
    }
}