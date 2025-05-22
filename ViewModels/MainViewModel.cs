using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using ArtGalleryStore.Models;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly CartService _cartService;

        private ProductsViewModel _productsViewModel;
        private CartViewModel _cartViewModel;
        private AuthViewModel _authViewModel;
        private UserProfileViewModel _userProfileViewModel;
        private AdminPanelViewModel _adminPanelViewModel;

        private object _currentView;
        
        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                // Добавляем логирование для отладки
                Debug.WriteLine($"Changing view to: {value?.GetType().Name}");
                SetProperty(ref _currentView, value); 
            }
        }

        public bool IsLoggedIn 
        { 
            get 
            {
                bool result = _authService.IsLoggedIn;
                Debug.WriteLine($"IsLoggedIn checked, result: {result}");
                return result;
            } 
        }
        
        public bool IsAdmin 
        { 
            get 
            {
                bool result = _authService.IsAdmin;
                Debug.WriteLine($"IsAdmin checked, result: {result}");
                return result;
            } 
        }
        
        public User CurrentUser => _authService.CurrentUser;
        
        // Свойства видимости для элементов интерфейса
        public Visibility AdminPanelVisibility 
        { 
            get 
            {
                Visibility result = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
                Debug.WriteLine($"AdminPanelVisibility checked, result: {result}");
                return result;
            } 
        }
        
        public Visibility ProfileButtonVisibility => IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LogoutButtonVisibility => IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoginButtonVisibility => !IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        
        // Команды для навигации
        public ICommand NavigateToProductsCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand NavigateToAuthCommand { get; }
        public ICommand NavigateToUserProfileCommand { get; }
        public ICommand NavigateToAdminPanelCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            _authService = App.AuthService;
            _productService = App.ProductService;
            _cartService = App.CartService;

            // Подписка на события
            _authService.CurrentUserChanged += OnCurrentUserChanged;

            // Инициализация команд
            NavigateToProductsCommand = new RelayCommand(_ => NavigateToProducts());
            NavigateToCartCommand = new RelayCommand(_ => NavigateToCart(), _ => IsLoggedIn);
            NavigateToAuthCommand = new RelayCommand(_ => NavigateToAuth(), _ => !IsLoggedIn);
            NavigateToUserProfileCommand = new RelayCommand(_ => NavigateToUserProfile(), _ => IsLoggedIn);
            NavigateToAdminPanelCommand = new RelayCommand(_ => NavigateToAdminPanel(), _ => IsAdmin);
            LogoutCommand = new RelayCommand(_ => Logout(), _ => IsLoggedIn);

            // Создаем ViewModels в правильном порядке и передаем все необходимые зависимости
            Debug.WriteLine("Initializing ViewModels in constructor");
            
            // Сначала инициализируем CartViewModel с правильными зависимостями
            _cartViewModel = new CartViewModel(_authService, _cartService);
            Debug.WriteLine($"CartViewModel initialized, hash: {_cartViewModel.GetHashCode()}");
            
            // Затем ProductsViewModel с ссылкой на CartViewModel
            _productsViewModel = new ProductsViewModel(_authService, _productService, _cartService)
            {
                CartViewModel = _cartViewModel
            };
            Debug.WriteLine($"ProductsViewModel initialized, hash: {_productsViewModel.GetHashCode()}, has CartViewModel: {_productsViewModel.CartViewModel != null}");
            
            // Проверяем, что корзины корректно инициализированы
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                var cart = _cartService.GetCart(_authService.CurrentUser.Id);
                Debug.WriteLine($"Current user cart has {cart.Items.Count} items");
                Debug.WriteLine($"CartViewModel shows IsCartEmpty={_cartViewModel.IsCartEmpty}");
            }
            
            // По умолчанию показываем каталог продуктов
            NavigateToProducts();
            
            Debug.WriteLine("MainViewModel constructor completed. Current view: " + 
                            (_currentView?.GetType().Name ?? "null"));
        }

        private void OnCurrentUserChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("OnCurrentUserChanged event fired");
            
            // Вывод информации о текущем пользователе
            if (_authService.CurrentUser != null)
            {
                Debug.WriteLine($"Current user: {_authService.CurrentUser.Username}, IsAdmin: {_authService.CurrentUser.IsAdmin}");
            }
            else
            {
                Debug.WriteLine("Current user is null (logged out)");
                
                // Если пользователь вышел и текущее представление требует авторизации,
                // перенаправляем на каталог продуктов
                if (CurrentView is CartViewModel || 
                    CurrentView is UserProfileViewModel || 
                    CurrentView is AdminPanelViewModel)
                {
                    Debug.WriteLine("User logged out while viewing protected view - redirecting to products");
                    NavigateToProducts();
                }
            }
            
            // Обновляем все связанные свойства
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CurrentUser));
            
            // Обновляем свойства видимости элементов UI
            OnPropertyChanged(nameof(AdminPanelVisibility));
            OnPropertyChanged(nameof(ProfileButtonVisibility));
            OnPropertyChanged(nameof(LogoutButtonVisibility));
            OnPropertyChanged(nameof(LoginButtonVisibility));

            // Обновляем команды для корректного отображения кнопок
            CommandManager.InvalidateRequerySuggested();
        }

        private void NavigateToProducts()
        {
            Debug.WriteLine("NavigateToProducts called");
            
            // Создаем новый экземпляр ProductsViewModel если необходимо
            if (_productsViewModel == null)
            {
                Debug.WriteLine("Creating new ProductsViewModel in NavigateToProducts");
                _productsViewModel = new ProductsViewModel(_authService, _productService, _cartService);
                
                // Важно установить ссылку на существующий CartViewModel
                if (_cartViewModel != null)
                {
                    _productsViewModel.CartViewModel = _cartViewModel;
                    Debug.WriteLine($"Connected CartViewModel (hash: {_cartViewModel.GetHashCode()}) to new ProductsViewModel");
                }
                else
                {
                    Debug.WriteLine("WARNING: CartViewModel is null when creating ProductsViewModel");
                }
            }
            else
            {
                Debug.WriteLine($"Using existing ProductsViewModel: {_productsViewModel.GetHashCode()}");
                
                // Ensure products are refreshed
                _productsViewModel.LoadProducts();
                Debug.WriteLine("Products refreshed");
            }
            
            CurrentView = _productsViewModel;
            Debug.WriteLine("Set CurrentView to ProductsViewModel");
        }

        private void NavigateToCart()
        {
            Debug.WriteLine("========== НАЧАЛО NavigateToCart ==========");
            Debug.WriteLine("NavigateToCart called, IsLoggedIn: " + IsLoggedIn);
            
            if (IsLoggedIn)
            {
                // Проверяем, существует ли уже экземпляр CartViewModel
                if (_cartViewModel == null)
                {
                    Debug.WriteLine("Creating new CartViewModel in NavigateToCart");
                    _cartViewModel = new CartViewModel(_authService, _cartService);
                    
                    // Обновляем ссылку в ProductsViewModel если она существует
                    if (_productsViewModel != null)
                    {
                        _productsViewModel.CartViewModel = _cartViewModel;
                        Debug.WriteLine("Updated CartViewModel reference in ProductsViewModel");
                    }
                }
                else
                {
                    Debug.WriteLine($"Using existing CartViewModel: {_cartViewModel.GetHashCode()}");
                    
                    // Перед обновлением содержимого выводим текущее состояние
                    Debug.WriteLine($"CartViewModel before refresh: IsCartEmpty={_cartViewModel.IsCartEmpty}, CartItems.Count={_cartViewModel.CartItems.Count}, Total={_cartViewModel.Total}");
                    
                    // Обновляем содержимое корзины
                    _cartViewModel.LoadCart();
                    Debug.WriteLine($"Cart items count after refresh: {_cartViewModel.CartItems.Count}");
                }
                
                // Переключаем отображение на корзину
                Debug.WriteLine("Устанавливаем CurrentView = CartViewModel");
                CurrentView = _cartViewModel;
                Debug.WriteLine($"CurrentView установлен на {CurrentView.GetType().Name}");
                
                // Дополнительная диагностика состояния корзины
                if (_authService.CurrentUser != null)
                {
                    var cart = _cartService.GetCart(_authService.CurrentUser.Id);
                    Debug.WriteLine($"Raw cart data from service: User ID: {_authService.CurrentUser.Id}, Items: {cart.Items.Count}");
                    
                    if (cart.Items.Count > 0)
                    {
                        Debug.WriteLine("Содержимое корзины из сервиса:");
                        foreach (var item in cart.Items)
                        {
                            Debug.WriteLine($"  Item: ID={item.Id}, ProductID={item.ProductId}, " +
                                          $"Product={item.Product?.Title ?? "null"}, Quantity={item.Quantity}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Корзина в сервисе пуста!");
                    }
                    
                    // Проверяем, что CartViewModel отображает те же данные, что и сервис
                    if (_cartViewModel.CartItems.Count != cart.Items.Count)
                    {
                        Debug.WriteLine($"!!! РАССИНХРОНИЗАЦИЯ: CartViewModel.CartItems.Count={_cartViewModel.CartItems.Count}, но CartService.Items.Count={cart.Items.Count}");
                    }
                    else
                    {
                        Debug.WriteLine("Количество элементов в CartViewModel и CartService совпадает");
                    }
                }
            }
            else 
            {
                Debug.WriteLine("Пользователь не авторизован, навигация в корзину невозможна");
            }
            
            Debug.WriteLine("========== КОНЕЦ NavigateToCart ==========");
        }

        private void NavigateToAuth()
        {
            Debug.WriteLine("NavigateToAuth called, IsLoggedIn: " + IsLoggedIn);
            if (!IsLoggedIn)
            {
                // Создаем новый экземпляр AuthViewModel если необходимо
                if (_authViewModel == null)
                {
                    _authViewModel = new AuthViewModel(_authService);
                }
                
                CurrentView = _authViewModel;
            }
        }

        private void NavigateToUserProfile()
        {
            Debug.WriteLine("NavigateToUserProfile called, IsLoggedIn: " + IsLoggedIn);
            if (IsLoggedIn)
            {
                // Создаем новый экземпляр UserProfileViewModel если необходимо
                if (_userProfileViewModel == null)
                {
                    _userProfileViewModel = new UserProfileViewModel(_authService);
                }
                
                CurrentView = _userProfileViewModel;
            }
        }

        private void NavigateToAdminPanel()
        {
            Debug.WriteLine("NavigateToAdminPanel called, IsAdmin: " + IsAdmin);
            if (IsAdmin)
            {
                // Создаем новый экземпляр AdminPanelViewModel если необходимо
                if (_adminPanelViewModel == null)
                {
                    _adminPanelViewModel = new AdminPanelViewModel(_productService);
                }
                
                CurrentView = _adminPanelViewModel;
            }
            else
            {
                Debug.WriteLine("User is not admin, navigation to admin panel not allowed");
            }
        }

        private void Logout()
        {
            Debug.WriteLine("Logout called");
            _authService.Logout();
            
            // Перенаправляем на страницу продуктов, так как она доступна всем
            NavigateToProducts();
            
            // Обновляем команды для корректного отображения кнопок
            CommandManager.InvalidateRequerySuggested();
        }
    }
}