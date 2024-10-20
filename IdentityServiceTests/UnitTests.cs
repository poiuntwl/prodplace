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

[Collection(nameof(IdentityServiceCollectionDefinition))]
public class UnitTests : TestFixtureBase, IClassFixture<RegisterUserFixture>
{
    private readonly RegisterDto _dto;
    private readonly UserDataResult? _user;
    private readonly Func<Task> _resetDb;

    public UnitTests(IdentityServiceFactory identityServiceFactory, RegisterUserFixture registerUserFixture) : base(identityServiceFactory)
    {
        _dto = registerUserFixture.Dto;
        _user = registerUserFixture.User;
        _resetDb = identityServiceFactory.ResetDbAsync;
    }

    public override async Task DisposeAsync()
    {
        await _resetDb();

        await base.DisposeAsync();
    }

    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        _user.Should().NotBeNull();
        _user!.Email.Should().Be(_dto.Email);
        _user.Username.Should().Be(_dto.Username);
        _user.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_ShouldContainValidToken()
    {
        var tokenBodyJson = JToken.Parse(Base64ToJson(_user.Token.Split('.')[1] + '='));
        tokenBodyJson.Value<string>("email").Should().Be(_dto.Email);
        tokenBodyJson.Value<string>("given_name").Should().Be(_dto.Username);
    }

    [Fact]
    public async Task Register_ShouldCreateUserInDatabase()
    {
        var userManager = ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        userManager.Users.ToList().Should().ContainSingle(x => x.Email == _dto.Email);
    }

    [Fact]
    public async Task Register_ShouldCreateOutboxMessageInDatabase()
    {
        var dbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        var foundMessage = dbContext.OutboxMessages.Should().ContainSingle(x => x.Content != null).Subject;
        var contentDeserialized = JsonSerializer.Deserialize<UserCreatedEventData>(foundMessage.Content);
        contentDeserialized.Email.Should().Be(_dto.Email);
        contentDeserialized.Username.Should().Be(_dto.Username);
        contentDeserialized.FirstName.Should().Be(_dto.Username);
    }

    private static string Base64ToJson(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}