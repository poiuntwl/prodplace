extern alias AdminSUT;
using AdminSUT::Prodplace.Admin;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using Keycloak.Net.Models.Users;

namespace IntegrationTests.Tests;

extern alias AdminSUT;

[Collection<IntegrationCollection>]
public class IdentityTests
{
    private readonly BaseIntegrationTestFixture _baseIntegrationTestFixture;

    public IdentityTests(BaseIntegrationTestFixture baseIntegrationTestFixture)
    {
        _baseIntegrationTestFixture = baseIntegrationTestFixture;
    }

    [Fact]
    public async Task Should_Successfully_Create_And_Assign_Role_To_Registered_User()
    {
        var newUserId = await RegisterUserAsync();
        newUserId.Should().NotBeNull();

        var roleName = TestDataGenerator.GenerateString();
        var roleDescription = TestDataGenerator.GenerateString();
        var newRole =
            await _baseIntegrationTestFixture.AdminServiceFactory.HttpClient.CreateRole(
                new CreateRoleRequestDto(roleName, roleDescription));
        newRole.Should().NotBeNull();

        var roleAssigned =
            await _baseIntegrationTestFixture.AdminServiceFactory.HttpClient.AssignRole(
                new AssignRoleRequestDto(newUserId, roleName));
        roleAssigned.Should().NotBeNull();
        roleAssigned.Success.Should().BeTrue();

        var client =
            (await _baseIntegrationTestFixture.IdentityServiceFactory.KeycloakClient
                .GetClientsAsync("master", q: "account", cancellationToken: TestContext.Current.CancellationToken))
            .First();
        var userRoles =
            await _baseIntegrationTestFixture.IdentityServiceFactory.KeycloakClient.GetClientRoleMappingsForUserAsync(
                "master", newUserId, client.Id, TestContext.Current.CancellationToken);
        userRoles.Select(x => x.Name).Should().Contain(roleName);
    }

    private async Task<string> RegisterUserAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = TestDataGenerator.GenerateEmail(),
            Password = TestDataGenerator.GeneratePassword(16),
            FirstName = TestDataGenerator.GenerateFirstName(),
            LastName = TestDataGenerator.GenerateLastName()
        };

        var response = await _baseIntegrationTestFixture.IdentityServiceFactory.KeycloakClient.CreateAndRetrieveUserIdAsync(
            "master", new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.Email,
                EmailVerified = true,
                Enabled = true,
                Credentials =
                [
                    new Credentials
                    {
                        Type = "password",
                        Temporary = false,
                        Value = registerDto.Password,
                    }
                ]
            });
        response.Should().NotBeNull();
        return response;
    }
}