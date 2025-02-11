extern alias AdminSUT;
using AdminSUT::Prodplace.Admin;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using Keycloak.Net.Models.Users;

namespace IntegrationTests;

extern alias AdminSUT;

[Collection(nameof(IntegrationCollection))]
public class IdentityTests
{
    private readonly IntegrationTestFixture _integrationTestFixture;

    public IdentityTests(IntegrationTestFixture integrationTestFixture)
    {
        _integrationTestFixture = integrationTestFixture;
    }

    [Fact]
    public async Task Test()
    {
        var newUserId = await RegisterUserAsync();
        newUserId.Should().NotBeNull();

        var roleName = TestDataGenerator.GenerateString();
        var newRole =
            await _integrationTestFixture.AdminServiceFactory.HttpClient.CreateRole(roleName);
        newRole.Should().NotBeNull();
        newRole.Name.Should().Be(roleName);

        var roleAssigned =
            await _integrationTestFixture.AdminServiceFactory.HttpClient.AssignRole(
                new AssignRoleRequestDto(newUserId, roleName));
        roleAssigned.Should().NotBeNull();
        roleAssigned.Success.Should().BeTrue();

        var userRoles =
            await _integrationTestFixture.IdentityServiceFactory.KeycloakClient.GetRealmRoleMappingsForUserAsync(
                "master", newUserId);
        userRoles.Select(x => x.Name).Should().Contain(roleName);
    }

    private async Task<string> RegisterUserAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = TestDataGenerator.GenerateEmail(),
            Password = TestDataGenerator.GeneratePassword(16)
        };

        var response = await _integrationTestFixture.IdentityServiceFactory.KeycloakClient.CreateAndRetrieveUserIdAsync(
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