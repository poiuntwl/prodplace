using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRoleNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "936beea8-ae32-4b76-b5fb-9b84e39e895b");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f7d58429-81dc-4d75-8022-e7195db570ad");

            migrationBuilder.InsertData(
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "18ac2c02-a136-4bd2-adfc-ff97d6472f25", null, "admin", "ADMIN" },
                    { "782ff602-7c26-4b17-9f06-3aec93c9ea84", null, "user", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18ac2c02-a136-4bd2-adfc-ff97d6472f25");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "782ff602-7c26-4b17-9f06-3aec93c9ea84");

            migrationBuilder.InsertData(
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "936beea8-ae32-4b76-b5fb-9b84e39e895b", null, "Admin", "ADMIN" },
                    { "f7d58429-81dc-4d75-8022-e7195db570ad", null, "User", "USER" }
                });
        }
    }
}
