using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyRatesService.Models;

public class CurrencyExchangeRateModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(3)]
    [Column(TypeName = "CHAR(3)")]
    public string CurrencyCode { get; set; }

    [Required] [StringLength(50)] public string CurrencyName { get; set; }

    [Required]
    [Column(TypeName = "DECIMAL(10, 4)")]
    public decimal ExchangeRate { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdated { get; set; }
}