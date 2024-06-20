﻿using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsService.Models;

public class CustomerDto
{
    public int Id { get; set; }
    [Column(TypeName = "nvarchar(20)")] public string FirstName { get; set; } = string.Empty;
    [Column(TypeName = "nvarchar(30)")] public string LastName { get; set; } = string.Empty;
    [Column(TypeName = "nvarchar(200)")] public string PhotoUrl { get; set; } = string.Empty;
    [Column(TypeName = "date")] public DateTime BirthDate { get; set; }
}