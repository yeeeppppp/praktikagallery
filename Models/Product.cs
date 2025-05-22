using System;

namespace ArtGalleryStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Medium { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Year { get; set; } = DateTime.Now.Year;
        public bool InStock { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}