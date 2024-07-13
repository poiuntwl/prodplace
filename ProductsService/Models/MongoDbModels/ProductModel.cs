using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductsService.Models.MongoDbModels;

public class ProductModel
{
    [BsonId, BsonRepresentation(BsonType.Int32), BsonElement("_id")]
    public int Id { get; set; }

    [BsonElement("name"), BsonRequired] public string Name { get; set; } = string.Empty;
    [BsonElement("description")] public string Description { get; set; } = string.Empty;

    [BsonElement("price"), BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("customFields"), BsonRepresentation(BsonType.String)]
    public string CustomFields { get; set; } = string.Empty;
}