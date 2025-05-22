using System;
using System.Diagnostics;
using System.Windows;
using ArtGalleryStore.Services;
using ArtGalleryStore.ViewModels;

namespace ArtGalleryStore
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Статические экземпляры сервисов для глобального доступа
        public static AuthService AuthService { get; private set; }
        public static ProductService ProductService { get; private set; }
        public static CartService CartService { get; private set; }
        
        // Главная модель представления
        public static MainViewModel MainViewModel { get; private set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Инициализация сервисов
            InitializeServices();
            
            // Инициализация главной модели представления
            MainViewModel = new MainViewModel();
        }
        
        private void InitializeServices()
        {
            Debug.WriteLine("Initializing services");
            
            // Инициализация сервисов в правильном порядке
            // Создаем сначала сервисы, не зависящие от других
            AuthService = new AuthService();
            ProductService = new ProductService();
            
            // Затем сервисы, зависящие от уже созданных
            CartService = new CartService();
            
            Debug.WriteLine("Services initialized");
            
            // Для отладки автоматически входим под администратором
            bool loginResult = AuthService.Login("admin", "admin123");
            Debug.WriteLine($"Auto login as admin result: {loginResult}, IsAdmin: {AuthService.IsAdmin}");
        }
    }
}