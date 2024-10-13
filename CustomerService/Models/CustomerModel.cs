using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Models;

[Index(nameof(Email), IsUnique = true)]
public class CustomerModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    public DateTime DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdated { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    [MaxLength(100)]
    public string City { get; set; }

    [MaxLength(100)]
    public string Country { get; set; }

    [MaxLength(20)]
    public string PostalCode { get; set; }
}
