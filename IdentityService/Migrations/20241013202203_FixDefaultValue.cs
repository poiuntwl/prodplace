using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class FixDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "631334dd-ae02-4aa0-926f-6311cc0745f7");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "960ec648-632a-4d80-98b8-a5fbbc2186eb");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDate",
                schema: "identity",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 10, 13, 20, 20, 0, 633, DateTimeKind.Utc).AddTicks(7766));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDate",
                schema: "identity",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 13, 20, 20, 0, 633, DateTimeKind.Utc).AddTicks(7766),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.InsertData(
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "631334dd-ae02-4aa0-926f-6311cc0745f7", null, "Admin", "ADMIN" },
                    { "960ec648-632a-4d80-98b8-a5fbbc2186eb", null, "User", "USER" }
                });
        }
    }
}
