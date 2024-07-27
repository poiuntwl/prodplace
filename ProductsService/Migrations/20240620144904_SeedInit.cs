using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsService.Migrations
{
    /// <inheritdoc />
    public partial class SeedInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "FirstName", "LastName", "PhotoUrl", "BirthDate" },
                values: new object[,]
                {
                    { "John", "Doe", "https://example.com/john.jpg", new DateTime(1990, 1, 15) },
                    { "Jane", "Smith", "https://example.com/jane.jpg", new DateTime(1985, 5, 20) },
                    { "Mike", "Johnson", "https://example.com/mike.jpg", new DateTime(1988, 11, 30) }
                });

            migrationBuilder.Sql(@"
                DECLARE @Customer1Id INT, @Customer2Id INT, @Customer3Id INT;

                SELECT @Customer1Id = Id FROM Customers WHERE FirstName = 'John' AND LastName = 'Doe';
                SELECT @Customer2Id = Id FROM Customers WHERE FirstName = 'Jane' AND LastName = 'Smith';
                SELECT @Customer3Id = Id FROM Customers WHERE FirstName = 'Mike' AND LastName = 'Johnson';

                INSERT INTO Orders (ProductId, CustomerId, OrderedAt)
                VALUES 
                (1, @Customer1Id, GETDATE()),
                (2, @Customer2Id, GETDATE()),
                (3, @Customer3Id, GETDATE()),
                (1, @Customer2Id, GETDATE()),
                (2, @Customer3Id, GETDATE());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5 });

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "FirstName",
                keyValues: new object[] { "John", "Jane", "Mike" });
        }
    }
}
