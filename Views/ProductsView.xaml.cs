using System;
using System.Windows;
using System.Windows.Controls;
using ArtGalleryStore.Services;
using ArtGalleryStore.ViewModels;

namespace ArtGalleryStore.Views
{
    public partial class ProductsView : UserControl
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        
        public ProductsView()
        {
            InitializeComponent();
            
            // Получаем сервисы из глобального контекста
            _cartService = App.CartService;
            _authService = App.AuthService;
        }
        
        /// <summary>
        /// Обработчик для тестовой кнопки добавления в корзину
        /// </summary>
        private void TestAddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null)
                {
                    // Получаем ID продукта
                    if (int.TryParse(button.Tag.ToString(), out int productId))
                    {
                        // Проверяем авторизацию
                        if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
                        {
                            MessageBox.Show("Пожалуйста, войдите в систему чтобы добавить товар в корзину", 
                                "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        
                        // Прямой вызов сервиса для добавления в корзину
                        bool success = _cartService.AddToCart(_authService.CurrentUser.Id, productId);
                        
                        // Детальное сообщение о результате
                        if (success)
                        {
                            // Получаем корзину для отображения деталей
                            var cart = _cartService.GetCart(_authService.CurrentUser.Id);
                            var detailsMsg = $"Товар успешно добавлен! Всего в корзине: {cart.Items.Count} товаров";
                            
                            // Выводим детали корзины
                            foreach (var item in cart.Items)
                            {
                                detailsMsg += Environment.NewLine + $"- {item.Product.Title} (x{item.Quantity})";
                            }
                            
                            MessageBox.Show(detailsMsg, "Добавлено в корзину", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить товар в корзину", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при добавлении товара в корзину: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}