using System.Text;
using System.Text.Json;
using CommonModels.OutboxModels;
using FluentAssertions;
using IdentityService.Data;
using IdentityService.Dtos;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace IdentityServiceTests;

public class UnitTests : TestFixtureBase, IClassFixture<IdentityServiceFactory>
{
    private readonly HttpClient _httpClient;

    public UnitTests(IdentityServiceFactory factory) : base(factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        var registerBody = new RegisterDto
        {
            Email = "someemail@gmail.com",
            Password = "Somevalidpassword1!",
            Username = "some_username"
        };
        var jsonBody = JsonSerializer.Serialize(registerBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/register");
        request.Content = content;
        using var result = await _httpClient.SendAsync(request);
        var responseJson = await result.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<UserDataResult>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        response.Should().NotBeNull();
        response!.Email.Should().Be(registerBody.Email);
        response.Username.Should().Be(registerBody.Username);
        response.Token.Should().NotBeNullOrWhiteSpace();

        var tokenBodyJson = JToken.Parse(Base64ToJson(response.Token.Split('.')[1] + '='));
        tokenBodyJson.Value<string>("email").Should().Be(registerBody.Email);
        tokenBodyJson.Value<string>("given_name").Should().Be(registerBody.Username);

        var userManager = ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        userManager.Users.ToList().Should().ContainSingle(x => x.Email == registerBody.Email);

        var dbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        var foundMessage = dbContext.OutboxMessages.Should().ContainSingle(x => x.Content != null).Subject;
        var contentDeserialized = JsonSerializer.Deserialize<UserCreatedEventData>(foundMessage.Content);
        contentDeserialized.Email.Should().Be(registerBody.Email);
        contentDeserialized.Username.Should().Be(registerBody.Username);
        contentDeserialized.FirstName.Should().Be(registerBody.Username);
    }

    private static string Base64ToJson(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}