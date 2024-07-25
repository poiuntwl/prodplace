using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PriceService.Models;

public class PriceModel
{
    [BsonId, BsonRepresentation(BsonType.Int32), BsonElement("_id")]
    public int Id { get; set; }

    [BsonElement("productId"), BsonRepresentation(BsonType.Int32), BsonRequired]
    public int ProductId { get; set; }

    [BsonElement("amount"), BsonRepresentation(BsonType.Decimal128), BsonRequired]
    public decimal Amount { get; set; }

    [BsonElement("currency"), BsonRepresentation(BsonType.String), BsonRequired]
    public string Currency { get; set; } = "USD";

    [BsonElement("effectiveDate"), BsonRepresentation(BsonType.DateTime), BsonRequired]
    public DateTime EffectiveDate { get; set; }

    [BsonElement("expirationDate"), BsonRepresentation(BsonType.DateTime)]
    public DateTime? ExpirationDate { get; set; }

    [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime), BsonRequired]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
    public DateTime? UpdatedAt { get; set; }
}