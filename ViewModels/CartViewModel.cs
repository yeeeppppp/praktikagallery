using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArtGalleryStore.Models;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class CartViewModel : ObservableObject, IDisposable
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        
        private ObservableCollection<CartItem> _cartItems = new ObservableCollection<CartItem>();
        private decimal _total;
        private bool _isCheckingOut;
        private string _shippingAddress = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _orderConfirmed;
        private Order? _completedOrder;
        
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set 
            {
                if (SetProperty(ref _cartItems, value))
                {
                    OnPropertyChanged(nameof(IsCartEmpty));
                    OnPropertyChanged(nameof(EmptyCartVisibility));
                    OnPropertyChanged(nameof(CartItemsVisibility));
                }
            }
        }
        
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }
        
        public bool IsCheckingOut
        {
            get => _isCheckingOut;
            set 
            {
                if (SetProperty(ref _isCheckingOut, value))
                {
                    OnPropertyChanged(nameof(CheckoutFormVisibility));
                    OnPropertyChanged(nameof(NotCheckoutFormVisibility));
                    OnPropertyChanged(nameof(CartSummaryVisibility));
                }
            }
        }
        
        public string ShippingAddress
        {
            get => _shippingAddress;
            set => SetProperty(ref _shippingAddress, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set 
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(ErrorMessageVisibility));
                }
            }
        }
        
        public bool OrderConfirmed
        {
            get => _orderConfirmed;
            set 
            {
                if (SetProperty(ref _orderConfirmed, value))
                {
                    OnPropertyChanged(nameof(OrderConfirmationVisibility));
                    OnPropertyChanged(nameof(CartSummaryVisibility));
                }
            }
        }
        
        public Order CompletedOrder
        {
            get => _completedOrder;
            set => SetProperty(ref _completedOrder, value);
        }
        
        public bool IsCartEmpty => CartItems == null || !CartItems.Any();
        
        // Свойства видимости
        public Visibility EmptyCartVisibility => IsCartEmpty ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CartItemsVisibility => !IsCartEmpty ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CheckoutFormVisibility => IsCheckingOut ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NotCheckoutFormVisibility => !IsCheckingOut ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CartSummaryVisibility => !IsCheckingOut && !OrderConfirmed ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OrderConfirmationVisibility => OrderConfirmed ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ErrorMessageVisibility => string.IsNullOrEmpty(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
        
        // Команды
        public ICommand IncreaseQuantityCommand { get; private set; }
        public ICommand DecreaseQuantityCommand { get; private set; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand ClearCartCommand { get; private set; }
        public ICommand CheckoutCommand { get; private set; }
        public ICommand PlaceOrderCommand { get; private set; }
        public ICommand ContinueShoppingCommand { get; private set; }
        public ICommand ClearErrorCommand { get; private set; }
        
        /// <summary>
        /// Обновляет все свойства, которые зависят от содержимого корзины
        /// </summary>
        private void UpdateCartDependentProperties()
        {
            System.Diagnostics.Debug.WriteLine("UpdateCartDependentProperties called");
            
            // Обновляем все свойства видимости
            OnPropertyChanged(nameof(IsCartEmpty));
            OnPropertyChanged(nameof(EmptyCartVisibility));
            OnPropertyChanged(nameof(CartItemsVisibility));
            OnPropertyChanged(nameof(CartSummaryVisibility));
            
            System.Diagnostics.Debug.WriteLine($"Updated visibility properties: IsCartEmpty={IsCartEmpty}, CartItemsVisibility={CartItemsVisibility}");
            
            // Обновляем команды для учета изменения состояния корзины
            CommandManager.InvalidateRequerySuggested();
        }
        
        // Конструктор с двумя параметрами - основной
        public CartViewModel(AuthService authService, CartService cartService)
        {
            System.Diagnostics.Debug.WriteLine($"Constructor CartViewModel called with AuthService={authService?.GetHashCode()}, CartService={cartService?.GetHashCode()}");
            _authService = authService;
            _cartService = cartService;
            
            // Подписываемся на изменения корзины
            _cartService.CartChanged += OnCartChanged;
            System.Diagnostics.Debug.WriteLine($"CartViewModel constructor (with injected service): Subscribed to CartChanged event");
            
            // Инициализация команд
            IncreaseQuantityCommand = new RelayCommand(param => IncreaseQuantity(param));
            DecreaseQuantityCommand = new RelayCommand(param => DecreaseQuantity(param));
            RemoveItemCommand = new RelayCommand(param => RemoveItem(param));
            ClearCartCommand = new RelayCommand(param => ClearCart(), param => !IsCartEmpty);
            CheckoutCommand = new RelayCommand(param => Checkout(), param => !IsCartEmpty);
            PlaceOrderCommand = new RelayCommand(param => PlaceOrder(), param => CanPlaceOrder());
            ContinueShoppingCommand = new RelayCommand(param => ContinueShopping());
            ClearErrorCommand = new RelayCommand(param => ErrorMessage = string.Empty);
            
            // Инициализация коллекции товаров
            System.Diagnostics.Debug.WriteLine("Инициализируем CartItems в конструкторе");
            CartItems = new ObservableCollection<CartItem>();
            
            // Загрузка начального содержимого корзины
            System.Diagnostics.Debug.WriteLine("Загружаем содержимое корзины в конструкторе");
            LoadCart();
            
            // Принудительно обновляем все свойства видимости
            System.Diagnostics.Debug.WriteLine("Принудительно обновляем свойства видимости в конструкторе");
            UpdateCartDependentProperties();
            
            System.Diagnostics.Debug.WriteLine($"Конструктор CartViewModel завершен. IsCartEmpty={IsCartEmpty}, CartItems.Count={CartItems.Count}");
        }
        
        public CartViewModel()
        {
            _cartService = App.CartService;
            _authService = App.AuthService;
            
            // Подписываемся на изменения корзины
            _cartService.CartChanged += OnCartChanged;
            
            // Инициализация команд
            IncreaseQuantityCommand = new RelayCommand(param => IncreaseQuantity(param));
            DecreaseQuantityCommand = new RelayCommand(param => DecreaseQuantity(param));
            RemoveItemCommand = new RelayCommand(param => RemoveItem(param));
            ClearCartCommand = new RelayCommand(param => ClearCart(), param => !IsCartEmpty);
            CheckoutCommand = new RelayCommand(param => Checkout(), param => !IsCartEmpty);
            PlaceOrderCommand = new RelayCommand(param => PlaceOrder(), param => CanPlaceOrder());
            ContinueShoppingCommand = new RelayCommand(param => ContinueShopping());
            ClearErrorCommand = new RelayCommand(param => ErrorMessage = string.Empty);
            
            // Загрузка корзины
            LoadCart();
        }
        
        private void LoadCart()
        {
            System.Diagnostics.Debug.WriteLine("LoadCart method called");
            
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("LoadCart aborted: User not logged in or CurrentUser is null");
                CartItems = new ObservableCollection<CartItem>();
                UpdateTotals();
                return;
            }
            
            int userId = _authService.CurrentUser.Id;
            System.Diagnostics.Debug.WriteLine($"Loading cart for user: {userId}, Username: {_authService.CurrentUser.Username}");
            
            try
            {
                var cart = _cartService.GetCart(userId);
                System.Diagnostics.Debug.WriteLine($"Retrieved cart with {cart.Items.Count} items, UserId: {cart.UserId}");
                
                if (cart.Items.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Cart items details:");
                    foreach (var item in cart.Items)
                    {
                        System.Diagnostics.Debug.WriteLine($"Item: Id={item.Id}, ProductId={item.ProductId}, " +
                                                         $"Product.Title={item.Product?.Title ?? "null"}, " +
                                                         $"Quantity={item.Quantity}");
                    }
                }
                
                // Вместо замены всей коллекции, обновляем содержимое существующей коллекции
                // Это лучше для привязок данных в UI
                CartItems.Clear();
                foreach (var item in cart.Items)
                {
                    CartItems.Add(item);
                }
                
                System.Diagnostics.Debug.WriteLine($"CartItems updated with {CartItems.Count} items");
                
                // Явно обновляем все свойства, зависящие от содержимого корзины
                UpdateCartDependentProperties();
                
                // Выводим отладочную информацию о _userCarts в CartService
                System.Diagnostics.Debug.WriteLine($"Total number of carts in CartService: {_cartService.UserCarts.Count}");
                foreach (var kvp in _cartService.UserCarts)
                {
                    System.Diagnostics.Debug.WriteLine($"Cart for UserId={kvp.Key}: {kvp.Value.Items.Count} items");
                }
                
                UpdateTotals();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart: {ex.Message}");
                ErrorMessage = $"Ошибка при загрузке корзины: {ex.Message}";
                CartItems = new ObservableCollection<CartItem>();
                UpdateTotals();
            }
        }
        
        private void UpdateTotals()
        {
            System.Diagnostics.Debug.WriteLine("UpdateTotals called");
            System.Diagnostics.Debug.WriteLine($"CartItems count: {CartItems.Count}");
            
            // Пересчитываем общую сумму
            decimal newTotal = CartItems.Sum(item => item.Product.Price * item.Quantity);
            System.Diagnostics.Debug.WriteLine($"New total: {newTotal}");
            
            // Обновляем свойство
            Total = newTotal;
            
            // Вызываем UpdateCartDependentProperties для обновления всех зависимых свойств
            UpdateCartDependentProperties();
            
            System.Diagnostics.Debug.WriteLine("UpdateTotals completed");
        }
        
        private void IncreaseQuantity(object param)
        {
            if (param is CartItem item)
            {
                try
                {
                    _cartService.UpdateCartItemQuantity(_authService.CurrentUser.Id, item.Id, item.Quantity + 1);
                    // Обновление произойдет в событии CartChanged
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Не удалось обновить количество товара: {ex.Message}";
                }
            }
        }
        
        private void DecreaseQuantity(object param)
        {
            if (param is CartItem item && item.Quantity > 1)
            {
                try
                {
                    _cartService.UpdateCartItemQuantity(_authService.CurrentUser.Id, item.Id, item.Quantity - 1);
                    // Обновление произойдет в событии CartChanged
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Не удалось обновить количество товара: {ex.Message}";
                }
            }
        }
        
        private void RemoveItem(object param)
        {
            if (param is CartItem item)
            {
                try
                {
                    _cartService.RemoveFromCart(_authService.CurrentUser.Id, item.Id);
                    // Обновление произойдет в событии CartChanged
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Не удалось удалить товар из корзины: {ex.Message}";
                }
            }
        }
        
        private void ClearCart()
        {
            try
            {
                _cartService.ClearCart(_authService.CurrentUser.Id);
                // Обновление произойдет в событии CartChanged
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не удалось очистить корзину: {ex.Message}";
            }
        }
        
        private void Checkout()
        {
            if (IsCartEmpty)
            {
                ErrorMessage = "Корзина пуста";
                return;
            }
            
            ErrorMessage = string.Empty;
            
            // Предварительно заполняем адрес доставки, если он сохранен
            ShippingAddress = _authService.CurrentUser.Name;
            
            // Переключаемся в режим оформления заказа
            IsCheckingOut = true;
        }
        
        private bool CanPlaceOrder()
        {
            return !IsCartEmpty && IsCheckingOut && !string.IsNullOrWhiteSpace(ShippingAddress);
        }
        
        private void PlaceOrder()
        {
            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                ErrorMessage = "Необходимо указать адрес доставки";
                return;
            }
            
            try
            {
                var order = _cartService.CreateOrder(_authService.CurrentUser.Id, ShippingAddress);
                
                if (order != null)
                {
                    CompletedOrder = order;
                    OrderConfirmed = true;
                    
                    // Корзина уже очищена в методе CreateOrder
                }
                else
                {
                    ErrorMessage = "Не удалось создать заказ";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при оформлении заказа: {ex.Message}";
            }
        }
        
        private void ContinueShopping()
        {
            // Сбрасываем состояние до начального
            OrderConfirmed = false;
            IsCheckingOut = false;
            ShippingAddress = string.Empty;
            CompletedOrder = null;
            ErrorMessage = string.Empty;
        }
        
        // Публичный метод для добавления товара в корзину, который можно вызывать из других ViewModels
        public bool AddProductToCart(int productId, int quantity = 1)
        {
            System.Diagnostics.Debug.WriteLine($"CartViewModel.AddProductToCart: ProductId={productId}, Quantity={quantity}");
            
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("CartViewModel.AddProductToCart failed: User not logged in or CurrentUser is null");
                return false;
            }
            
            var result = _cartService.AddToCart(_authService.CurrentUser.Id, productId, quantity);
            System.Diagnostics.Debug.WriteLine($"CartViewModel.AddProductToCart result: {result}");
            
            return result;
        }
        
        private void OnCartChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("========== НАЧАЛО OnCartChanged ==========");
            System.Diagnostics.Debug.WriteLine($"OnCartChanged event received from {sender?.GetType().Name ?? "unknown"}");
            
            try
            {
                if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("Пользователь не авторизован, обновление корзины не требуется");
                    return;
                }
                
                int userId = _authService.CurrentUser.Id;
                System.Diagnostics.Debug.WriteLine($"Текущий пользователь: ID={userId}, Username={_authService.CurrentUser.Username}");
                
                // Информация о текущем состоянии корзины до обновления
                System.Diagnostics.Debug.WriteLine($"Состояние корзины ДО обновления: {CartItems.Count} товаров, EmptyCartVisibility={EmptyCartVisibility}, CartItemsVisibility={CartItemsVisibility}");
                
                // Выводим все корзины в CartService
                if (_cartService.UserCarts.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Корзины в CartService:");
                    foreach(var kvp in _cartService.UserCarts)
                    {
                        System.Diagnostics.Debug.WriteLine($"  UserId={kvp.Key}, Items={kvp.Value.Items.Count}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Корзины в CartService отсутствуют!");
                }
                
                // Используем Dispatcher для перехода в UI поток, если этот метод вызывается из другого потока
                Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Начало Dispatcher.Invoke");
                    
                    LoadCart();
                    
                    // Явно обновляем все свойства, которые зависят от состояния корзины
                    OnPropertyChanged(nameof(IsCartEmpty));
                    OnPropertyChanged(nameof(EmptyCartVisibility));
                    OnPropertyChanged(nameof(CartItemsVisibility));
                    OnPropertyChanged(nameof(CartSummaryVisibility));
                    
                    // Обновляем команды, чтобы учесть изменение состояния корзины
                    CommandManager.InvalidateRequerySuggested();
                    
                    System.Diagnostics.Debug.WriteLine($"Cart updated after CartChanged event: {CartItems.Count} items, Total: {Total}");
                    System.Diagnostics.Debug.WriteLine($"Новые значения свойств: IsCartEmpty={IsCartEmpty}, EmptyCartVisibility={EmptyCartVisibility}, CartItemsVisibility={CartItemsVisibility}");
                    
                    System.Diagnostics.Debug.WriteLine("Завершение Dispatcher.Invoke");
                });
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в OnCartChanged: {ex.Message}\n{ex.StackTrace}");
            }
            
            System.Diagnostics.Debug.WriteLine("========== ЗАВЕРШЕНИЕ OnCartChanged ==========");
        }
        
        // Освобождение ресурсов и отписка от событий
        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("CartViewModel.Dispose called");
            
            // Отписываемся от событий
            if (_cartService != null)
            {
                _cartService.CartChanged -= OnCartChanged;
                System.Diagnostics.Debug.WriteLine("Unsubscribed from CartService.CartChanged event");
            }
        }
    }
}