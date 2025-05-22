using System;
using System.Collections.Generic;
using System.Linq;
using ArtGalleryStore.Models;

namespace ArtGalleryStore.Services
{
    public class AuthService
    {
        private List<User> _users;
        private User _currentUser;
        
        public User CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;
        public bool IsAdmin => _currentUser?.IsAdmin ?? false;

        // Событие для оповещения о смене пользователя
        public event EventHandler CurrentUserChanged;
        
        public AuthService()
        {
            InitializeUsers();
        }
        
        private void InitializeUsers()
        {
            // В реальном приложении это должно быть загружено из базы данных
            _users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123", // В реальном приложении пароль должен быть хеширован
                    Email = "admin@artstore.com",
                    Name = "Администратор",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Password = "user123", // В реальном приложении пароль должен быть хеширован
                    Email = "user@example.com",
                    Name = "Пользователь",
                    Role = UserRole.User,
                    CreatedAt = DateTime.Now.AddDays(-15)
                }
            };
        }
        
        public bool Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            
            if (user != null)
            {
                _currentUser = user;
                CurrentUserChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            
            return false;
        }
        
        public void Logout()
        {
            _currentUser = null;
            CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public bool Register(string username, string password, string email, string name)
        {
            // Проверка, существует ли уже такой пользователь
            if (_users.Any(u => u.Username == username || u.Email == email))
            {
                return false;
            }
            
            var newUser = new User
            {
                Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                Username = username,
                Password = password, // В реальном приложении пароль должен быть хеширован
                Email = email,
                Name = name,
                Role = UserRole.User,
                CreatedAt = DateTime.Now
            };
            
            _users.Add(newUser);
            
            // Автоматически входим под новым пользователем
            _currentUser = newUser;
            CurrentUserChanged?.Invoke(this, EventArgs.Empty);
            
            return true;
        }
    }
}