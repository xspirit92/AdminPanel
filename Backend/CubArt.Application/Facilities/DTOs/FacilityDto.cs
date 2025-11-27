using CubArt.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Facilities.DTOs
{
    public class FacilityDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Address { get; set; }
    }
}
