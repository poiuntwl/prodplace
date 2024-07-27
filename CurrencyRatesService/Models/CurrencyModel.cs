using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyRatesService.Models;

public class CurrencyExchangeRateModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(12)]
    [Column(TypeName = "CHAR(12)")]
    public string CurrencyCode { get; set; }

    [StringLength(50)] public string? CurrencyName { get; set; }

    [Required]
    [Column(TypeName = "DECIMAL(20, 8)")]
    public decimal ExchangeRate { get; set; }

    public DateTime LastUpdated { get; set; }
}