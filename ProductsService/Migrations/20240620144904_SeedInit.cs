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

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Name", "Description", "Price", "CustomFields" },
                values: new object[,]
                {
                    { "Laptop", "High-performance laptop", 999.99m, "{\"color\": \"silver\", \"RAM\": \"16GB\"}" },
                    { "Smartphone", "Latest model smartphone", 699.99m, "{\"color\": \"black\", \"storage\": \"128GB\"}" },
                    { "Headphones", "Noise-cancelling headphones", 249.99m, "{\"color\": \"white\", \"type\": \"over-ear\"}" }
                });

            migrationBuilder.Sql(@"
                DECLARE @Customer1Id INT, @Customer2Id INT, @Customer3Id INT;
                DECLARE @Product1Id INT, @Product2Id INT, @Product3Id INT;

                SELECT @Customer1Id = Id FROM Customers WHERE FirstName = 'John' AND LastName = 'Doe';
                SELECT @Customer2Id = Id FROM Customers WHERE FirstName = 'Jane' AND LastName = 'Smith';
                SELECT @Customer3Id = Id FROM Customers WHERE FirstName = 'Mike' AND LastName = 'Johnson';

                SELECT @Product1Id = Id FROM Products WHERE Name = 'Laptop';
                SELECT @Product2Id = Id FROM Products WHERE Name = 'Smartphone';
                SELECT @Product3Id = Id FROM Products WHERE Name = 'Headphones';

                INSERT INTO Purchases (ProductId, CustomerId)
                VALUES 
                (@Product1Id, @Customer1Id),
                (@Product2Id, @Customer2Id),
                (@Product3Id, @Customer3Id),
                (@Product1Id, @Customer2Id),
                (@Product2Id, @Customer3Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Purchases",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5 });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Name",
                keyValues: new object[] { "Laptop", "Smartphone", "Headphones" });

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "FirstName",
                keyValues: new object[] { "John", "Jane", "Mike" });
        }
    }
}
