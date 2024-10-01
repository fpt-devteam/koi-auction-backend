using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Models;

namespace AuctionManagementService.Dto.Lot
{
    public class UpdateLotDto
    {
        [Required]
        public decimal StartingPrice { get; set; }
        [Required]
        public int AuctionMethodId { get; set; }
        [Required]
        public string Variety { get; set; } = null!;

        [Required]
        public bool Sex { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Size must be greater than 0")]
        public int SizeCm { get; set; }

        [Required]
        public int YearOfBirth { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
        public decimal WeightKg { get; set; }

    }
}