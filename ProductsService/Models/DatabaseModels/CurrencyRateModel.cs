using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsService.Models.DatabaseModels;

public class CurrencyRateModel
{
    [Key] public int Id { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; }

    [Required] [StringLength(50)] public string CurrencyName { get; set; }

    [Required]
    [Column(TypeName = "decimal(19,6)")]
    public decimal ExchangeRate { get; set; }

    [Required]
    [Column(TypeName = "datetime2(2)")]
    public DateTime LastUpdated { get; set; }
}