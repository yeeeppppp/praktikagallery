using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtGalleryStore.Models
{
    public class Cart
    {
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public bool IsEmpty => !Items.Any();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal Total => Items.Sum(i => i.Quantity * i.Product.Price);
    }
}