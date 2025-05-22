using System;

namespace ArtGalleryStore.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Вспомогательное свойство для простой проверки роли администратора
        public bool IsAdmin => Role == UserRole.Admin;
    }
}