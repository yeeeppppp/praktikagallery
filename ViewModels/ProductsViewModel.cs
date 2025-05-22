using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArtGalleryStore.Models;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class ProductsViewModel : ObservableObject, IDisposable
    {
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product? _selectedProduct;
        
        // Фильтры
        private string _searchQuery = string.Empty;
        private string _selectedMedium = string.Empty;
        private decimal? _minPrice;
        private decimal? _maxPrice;
        private bool? _inStockOnly;
        
        // Сортировка
        private string _sortOption = "По умолчанию";
        
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        
        public ObservableCollection<Product> Products
        {
            get => _products;
            set 
            {
                if (SetProperty(ref _products, value))
                {
                    OnPropertyChanged(nameof(NoProductsVisibility));
                }
            }
        }
        
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set 
            { 
                if (SetProperty(ref _selectedProduct, value))
                {
                    OnPropertyChanged(nameof(ProductDetailsVisibility));
                    OnPropertyChanged(nameof(GetOutOfStockVisibility));
                }
            }
        }
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFilters();
                }
            }
        }
        
        public string SelectedMedium
        {
            get => _selectedMedium;
            set
            {
                if (SetProperty(ref _selectedMedium, value))
                {
                    ApplyFilters();
                }
            }
        }
        
        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                if (SetProperty(ref _minPrice, value))
                {
                    ApplyFilters();
                }
            }
        }
        
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (SetProperty(ref _maxPrice, value))
                {
                    ApplyFilters();
                }
            }
        }
        
        public bool? InStockOnly
        {
            get => _inStockOnly;
            set
            {
                if (SetProperty(ref _inStockOnly, value))
                {
                    ApplyFilters();
                }
            }
        }
        
        public string SortOption
        {
            get => _sortOption;
            set
            {
                if (SetProperty(ref _sortOption, value))
                {
                    ApplySorting();
                }
            }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set 
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                }
            }
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
        
        public string SuccessMessage
        {
            get => _successMessage;
            set 
            {
                if (SetProperty(ref _successMessage, value))
                {
                    OnPropertyChanged(nameof(SuccessMessageVisibility));
                }
            }
        }
        
        // Свойства видимости
        public Visibility ProductDetailsVisibility => SelectedProduct != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoProductsVisibility => Products?.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ErrorMessageVisibility => string.IsNullOrEmpty(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SuccessMessageVisibility => string.IsNullOrEmpty(SuccessMessage) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AddToCartButtonVisibility => _authService?.IsLoggedIn == true ? Visibility.Visible : Visibility.Collapsed;
        
        // Метод для определения видимости плашки "Нет в наличии" 
        // Используется в ProductsView.xaml через конвертер
        public bool GetOutOfStockVisibility => SelectedProduct != null && !SelectedProduct.InStock;
        
        public List<string> AvailableMediums { get; private set; } = new List<string>();
        public List<string> SortOptions { get; } = new List<string>
        {
            "По умолчанию",
            "Цена (по возрастанию)",
            "Цена (по убыванию)",
            "Название (А-Я)",
            "Название (Я-А)"
        };
        
        // Команды
        public ICommand AddToCartCommand { get; private set; }
        public ICommand ResetFiltersCommand { get; private set; }
        public ICommand ViewProductDetailsCommand { get; private set; }
        public ICommand CloseProductDetailsCommand { get; private set; }
        
        private readonly CartViewModel _cartViewModel;
        
        public ProductsViewModel(ProductService productService) : this(productService, null)
        {
        }
        
        public ProductsViewModel(ProductService productService, CartViewModel cartViewModel)
        {
            _productService = productService;
            _cartService = App.CartService;
            _authService = App.AuthService;
            _cartViewModel = cartViewModel;
            
            System.Diagnostics.Debug.WriteLine($"ProductsViewModel constructor with CartViewModel: {_cartViewModel != null}");
        }
        
        public ProductsViewModel()
        {
            _productService = App.ProductService;
            _cartService = App.CartService;
            _authService = App.AuthService;
            
            Products = new ObservableCollection<Product>();
            
            // Подписываемся на изменения в коллекции продуктов
            _productService.ProductsChanged += OnProductsChanged;
            
            // Подписываемся на изменение пользователя для обновления видимости кнопок
            _authService.CurrentUserChanged += OnCurrentUserChanged;
            
            // Инициализация команд
            AddToCartCommand = new RelayCommand(param => AddToCart(param), param => CanAddToCart(param));
            ResetFiltersCommand = new RelayCommand(param => ResetFilters());
            ViewProductDetailsCommand = new RelayCommand(param => ViewProductDetails(param));
            CloseProductDetailsCommand = new RelayCommand(param => SelectedProduct = null);
            
            // Загрузка начальных данных
            LoadProducts();
            LoadMediums();
            
            // По умолчанию сортируем "По умолчанию"
            _sortOption = SortOptions[0];
        }
        
        private void OnCurrentUserChanged(object sender, EventArgs e)
        {
            // Обновляем свойство видимости кнопки добавления в корзину
            OnPropertyChanged(nameof(AddToCartButtonVisibility));
            
            // Обновляем состояние всех команд
            CommandManager.InvalidateRequerySuggested();
        }
        
        private void LoadProducts()
        {
            IsLoading = true;
            ErrorMessage = null;
            
            try
            {
                var products = _productService.FilterProducts();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке продуктов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void LoadMediums()
        {
            try
            {
                AvailableMediums = _productService.GetAllProducts()
                    .Select(p => p.Medium)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();
                
                OnPropertyChanged(nameof(AvailableMediums));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке фильтров: {ex.Message}";
            }
        }
        
        private void ApplyFilters()
        {
            IsLoading = true;
            ErrorMessage = null;
            
            try
            {
                var filteredProducts = _productService.FilterProducts(
                    SearchQuery,
                    SelectedMedium,
                    MinPrice,
                    MaxPrice,
                    InStockOnly
                );
                
                Products = new ObservableCollection<Product>(filteredProducts);
                ApplySorting();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при фильтрации: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void ApplySorting()
        {
            if (Products == null || !Products.Any())
                return;
                
            IOrderedEnumerable<Product> sortedProducts = null;
            
            switch (SortOption)
            {
                case "Цена (по возрастанию)":
                    sortedProducts = Products.OrderBy(p => p.Price);
                    break;
                case "Цена (по убыванию)":
                    sortedProducts = Products.OrderByDescending(p => p.Price);
                    break;
                case "Название (А-Я)":
                    sortedProducts = Products.OrderBy(p => p.Title);
                    break;
                case "Название (Я-А)":
                    sortedProducts = Products.OrderByDescending(p => p.Title);
                    break;
                default:
                    // По умолчанию сортируем по ID (как добавлены в систему)
                    sortedProducts = Products.OrderBy(p => p.Id);
                    break;
            }
            
            Products = new ObservableCollection<Product>(sortedProducts);
        }
        
        private void ResetFilters()
        {
            SearchQuery = null;
            SelectedMedium = null;
            MinPrice = null;
            MaxPrice = null;
            InStockOnly = null;
            SortOption = SortOptions[0];
            
            LoadProducts();
        }
        
        private bool CanAddToCart(object param)
        {
            if (param is Product product)
            {
                bool isLoggedIn = _authService?.IsLoggedIn ?? false;
                bool isInStock = product?.InStock ?? false;
                
                System.Diagnostics.Debug.WriteLine($"CanAddToCart check: IsLoggedIn={isLoggedIn}, ProductId={product?.Id}, InStock={isInStock}");
                
                // Уведомляем об изменении свойства видимости при изменении статуса авторизации
                OnPropertyChanged(nameof(AddToCartButtonVisibility));
                
                return isLoggedIn && isInStock;
            }
            
            System.Diagnostics.Debug.WriteLine("CanAddToCart check: param is not a Product");
            return false;
        }
        
        private void AddToCart(object param)
        {
            try 
            {
                // Детальное логирование всех шагов
                System.Diagnostics.Debug.WriteLine("=== НАЧАЛО AddToCart ===");
                System.Diagnostics.Debug.WriteLine($"AddToCart вызван с типом параметра: {param?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"Текущий пользователь: {(_authService?.CurrentUser != null ? _authService.CurrentUser.Id.ToString() : "null")}");
                System.Diagnostics.Debug.WriteLine($"CartViewModel установлен: {(_cartViewModel != null ? "да" : "нет")}");
                System.Diagnostics.Debug.WriteLine($"CartService установлен: {(_cartService != null ? "да" : "нет")}");
                System.Diagnostics.Debug.WriteLine($"Количество корзин в CartService: {(_cartService?.UserCarts?.Count.ToString() ?? "null")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при логировании начала AddToCart: {ex.Message}");
            }
            
            try
            {
                if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
                {
                    ErrorMessage = "Необходимо авторизоваться для добавления товаров в корзину";
                    System.Diagnostics.Debug.WriteLine("AddToCart failed: User not logged in or CurrentUser is null");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"Current User: Id={_authService.CurrentUser.Id}, Name={_authService.CurrentUser.Name}");
                
                if (param is Product product)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding product to cart: Id={product.Id}, Title={product.Title}, InStock={product.InStock}");
                    
                    if (!product.InStock)
                    {
                        ErrorMessage = "Товар недоступен для заказа";
                        System.Diagnostics.Debug.WriteLine("AddToCart failed: Product not in stock");
                        return;
                    }
                    
                    // Используем CartViewModel или прямой вызов CartService в зависимости от доступности
                    bool success = false;
                    
                    if (_cartViewModel != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Using CartViewModel to add product to cart");
                        System.Diagnostics.Debug.WriteLine($"UserID={_authService.CurrentUser.Id}, ProductID={product.Id}");
                        success = _cartViewModel.AddProductToCart(product.Id);
                        System.Diagnostics.Debug.WriteLine($"Результат добавления через CartViewModel: {success}");
                    }
                    else 
                    {
                        System.Diagnostics.Debug.WriteLine("Using CartService directly to add product to cart");
                        System.Diagnostics.Debug.WriteLine($"UserID={_authService.CurrentUser.Id}, ProductID={product.Id}");
                        success = _cartService.AddToCart(_authService.CurrentUser.Id, product.Id);
                        System.Diagnostics.Debug.WriteLine($"Результат добавления через CartService: {success}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"AddToCart result: {success}");
                    
                    if (success)
                    {
                        // Создаем сообщение с детальной информацией о корзине
                        var cartDetails = new System.Text.StringBuilder();
                        cartDetails.AppendLine("Товар добавлен в корзину!");
                        
                        // Показываем состояние корзины прямо в уведомлении
                        var cart = _cartService.GetCart(_authService.CurrentUser.Id);
                        cartDetails.AppendLine($"Товаров в корзине: {cart.Items.Count}");
                        
                        if (cart.Items.Count > 0)
                        {
                            cartDetails.AppendLine("Содержимое корзины:");
                            foreach (var item in cart.Items)
                            {
                                cartDetails.AppendLine($"- {item.Product.Title} (x{item.Quantity}) - {item.Product.Price:N0} ₽");
                            }
                            cartDetails.AppendLine($"Итого: {cart.Total:N0} ₽");
                        }
                        
                        // Временное уведомление с подробной информацией о корзине
                        SuccessMessage = cartDetails.ToString();
                        OnPropertyChanged(nameof(SuccessMessage));
                        OnPropertyChanged(nameof(SuccessMessageVisibility));
                        
                        // Выводим отладочную информацию о _userCarts в CartService после добавления
                        System.Diagnostics.Debug.WriteLine($"AFTER ADD: Total number of carts in CartService: {_cartService.UserCarts.Count}");
                        foreach (var kvp in _cartService.UserCarts)
                        {
                            System.Diagnostics.Debug.WriteLine($"AFTER ADD: Cart for UserId={kvp.Key}: {kvp.Value.Items.Count} items");
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Success message set: {SuccessMessage}");
                        
                        // Сбрасываем сообщение об ошибке, если оно было показано ранее
                        ErrorMessage = string.Empty;
                        
                        // Запускаем таймер для скрытия сообщения через 10 секунд
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                            System.Threading.Tasks.Task.Delay(10000).ContinueWith(_ => 
                            {
                                Application.Current.Dispatcher.Invoke(() => {
                                    // Проверяем, что сообщение начинается с "Товар добавлен в корзину"
                                    if (SuccessMessage.StartsWith("Товар добавлен в корзину"))
                                    {
                                        SuccessMessage = string.Empty;
                                        OnPropertyChanged(nameof(SuccessMessage));
                                        OnPropertyChanged(nameof(SuccessMessageVisibility));
                                        System.Diagnostics.Debug.WriteLine("Cart details message cleared");
                                    }
                                });
                            });
                        }));
                    }
                    else
                    {
                        ErrorMessage = "Не удалось добавить товар в корзину";
                        OnPropertyChanged(nameof(ErrorMessage));
                        OnPropertyChanged(nameof(ErrorMessageVisibility));
                        System.Diagnostics.Debug.WriteLine("AddToCart failed: CartService returned false");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AddToCart failed: param is not a Product");
                    ErrorMessage = "Ошибка добавления товара в корзину: неверный тип объекта";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in AddToCart: {ex.Message}");
                ErrorMessage = $"Ошибка при добавлении товара: {ex.Message}";
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }
        
        private void ViewProductDetails(object param)
        {
            System.Diagnostics.Debug.WriteLine($"ViewProductDetails called with param type: {param?.GetType().Name ?? "null"}");
            
            try
            {
                if (param is Product product)
                {
                    System.Diagnostics.Debug.WriteLine($"Viewing details for product: Id={product.Id}, Title={product.Title}");
                    SelectedProduct = product;
                    
                    // Уведомляем об изменении свойства видимости
                    OnPropertyChanged(nameof(ProductDetailsVisibility));
                    OnPropertyChanged(nameof(GetOutOfStockVisibility));
                    
                    System.Diagnostics.Debug.WriteLine($"ProductDetailsVisibility updated: {ProductDetailsVisibility}");
                    
                    // Обновляем состояние кнопок
                    CommandManager.InvalidateRequerySuggested();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ViewProductDetails failed: param is not a Product");
                    ErrorMessage = "Ошибка: не удалось открыть информацию о товаре";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in ViewProductDetails: {ex.Message}");
                ErrorMessage = $"Ошибка при просмотре товара: {ex.Message}";
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }
        
        private void OnProductsChanged(object sender, EventArgs e)
        {
            // Перезагружаем продукты при изменении в сервисе
            ApplyFilters();
        }
        
        public void Dispose()
        {
            // Отписываемся от событий
            _productService.ProductsChanged -= OnProductsChanged;
            _authService.CurrentUserChanged -= OnCurrentUserChanged;
            
            System.Diagnostics.Debug.WriteLine("ProductsViewModel: Disposed and unsubscribed from events");
        }
    }
}