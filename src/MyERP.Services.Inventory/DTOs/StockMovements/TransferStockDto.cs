using System;
using System.ComponentModel.DataAnnotations;

namespace MyERP.Services.Inventory.DTOs.StockMovements
{
    public class TransferStockDto
    {
        [Required]
        public string ItemType { get; set; } = string.Empty; // "Product" or "RawMaterial"
        
        [Required]
        public Guid ItemId { get; set; }
        
        [Required]
        public Guid FromStorageLocationId { get; set; }
        
        [Required]
        public Guid ToStorageLocationId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }
        
        public string? Notes { get; set; }
    }
}
