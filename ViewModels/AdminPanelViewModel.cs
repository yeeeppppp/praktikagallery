using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArtGalleryStore.Models;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class AdminPanelViewModel : ObservableObject, IDisposable
    {
        private readonly ProductService _productService;
        private readonly AuthService _authService;
        
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product? _selectedProduct;
        private Product? _editingProduct;
        private bool _isAddingNew;
        private bool _isEditing;
        private string _errorMessage = string.Empty;
        
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }
        
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    // Сбрасываем состояние редактирования при смене выбора
                    if (_isEditing || _isAddingNew)
                    {
                        CancelEdit();
                    }
                }
            }
        }
        
        public Product EditingProduct
        {
            get => _editingProduct;
            set => SetProperty(ref _editingProduct, value);
        }
        
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set 
            {
                if (SetProperty(ref _isAddingNew, value))
                {
                    OnPropertyChanged(nameof(EditingVisibility));
                    OnPropertyChanged(nameof(NotEditingVisibility));
                    OnPropertyChanged(nameof(FormTitle));
                }
            }
        }
        
        public bool IsEditing
        {
            get => _isEditing;
            set 
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(EditingVisibility));
                    OnPropertyChanged(nameof(NotEditingVisibility));
                    OnPropertyChanged(nameof(FormTitle));
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
        
        // Свойства видимости
        public Visibility ErrorMessageVisibility => 
            string.IsNullOrEmpty(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
            
        public Visibility EditingVisibility => 
            IsEditing || IsAddingNew ? Visibility.Visible : Visibility.Collapsed;
            
        public Visibility NotEditingVisibility => 
            !IsEditing && !IsAddingNew ? Visibility.Visible : Visibility.Collapsed;
            
        public string FormTitle => 
            IsAddingNew ? "Добавить новый товар" : "Редактировать товар";
            
        // Текстовые свойства для отображения
        public string InStockText(bool inStock) => inStock ? "В наличии" : "Нет в наличии";
        
        // Цветовые свойства
        public string InStockColor(bool inStock) => inStock ? "SecondaryBrush" : "AccentBrush";
        
        // Команды
        public ICommand AddNewProductCommand { get; private set; }
        public ICommand EditProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand SaveProductCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        
        public AdminPanelViewModel(ProductService productService)
        {
            _productService = productService;
            _authService = App.AuthService;
            
            // Инициализация команд
            AddNewProductCommand = new RelayCommand(_ => AddNewProduct(), _ => CanAddNewProduct());
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => CanEditProduct());
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => CanDeleteProduct());
            SaveProductCommand = new RelayCommand(_ => SaveProduct(), _ => CanSaveProduct());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
        }
        
        public AdminPanelViewModel()
        {
            _productService = App.ProductService;
            _authService = App.AuthService;
            
            // Подписываемся на изменения продуктов
            _productService.ProductsChanged += OnProductsChanged;
            
            // Инициализация команд
            AddNewProductCommand = new RelayCommand(_ => AddNewProduct(), _ => CanAddNewProduct());
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => CanEditProduct());
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => CanDeleteProduct());
            SaveProductCommand = new RelayCommand(_ => SaveProduct(), _ => CanSaveProduct());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            
            // Загрузка продуктов
            LoadProducts();
        }
        
        private void LoadProducts()
        {
            try
            {
                var products = _productService.GetAllProducts();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке товаров: {ex.Message}";
            }
        }
        
        private bool CanAddNewProduct()
        {
            bool canAdd = _authService.IsAdmin && !IsEditing && !IsAddingNew;
            System.Diagnostics.Debug.WriteLine($"CanAddNewProduct: {canAdd} (IsAdmin={_authService.IsAdmin}, IsEditing={IsEditing}, IsAddingNew={IsAddingNew})");
            return canAdd;
        }
        
        private void AddNewProduct()
        {
            System.Diagnostics.Debug.WriteLine("AddNewProduct called");
            
            if (!_authService.IsAdmin)
            {
                ErrorMessage = "Доступ запрещен. Требуются права администратора.";
                System.Diagnostics.Debug.WriteLine($"Нет прав администратора. IsAdmin: {_authService.IsAdmin}");
                return;
            }
            
            IsAddingNew = true;
            IsEditing = false;
            
            System.Diagnostics.Debug.WriteLine($"IsAddingNew: {IsAddingNew}, IsEditing: {IsEditing}");
            
            EditingProduct = new Product
            {
                Title = "Новая картина",
                Description = "Описание картины",
                Price = 0,
                ImageUrl = "",
                Artist = "",
                Medium = "",
                Size = "",
                Year = DateTime.Now.Year,
                InStock = true,
                CreatedAt = DateTime.Now
            };
            
            System.Diagnostics.Debug.WriteLine($"EditingProduct created: {EditingProduct.Title}");
            System.Diagnostics.Debug.WriteLine($"EditingVisibility: {EditingVisibility}, NotEditingVisibility: {NotEditingVisibility}");
            
            // Обновляем видимость
            OnPropertyChanged(nameof(EditingVisibility));
            OnPropertyChanged(nameof(NotEditingVisibility));
            
            // Обновляем состояние команд
            InvalidateCommands();
        }
        
        private bool CanEditProduct()
        {
            return _authService.IsAdmin && SelectedProduct != null && !IsEditing && !IsAddingNew;
        }
        
        private void EditProduct()
        {
            if (!_authService.IsAdmin)
            {
                ErrorMessage = "Доступ запрещен. Требуются права администратора.";
                return;
            }
            
            if (SelectedProduct == null)
            {
                ErrorMessage = "Выберите товар для редактирования";
                return;
            }
            
            IsEditing = true;
            IsAddingNew = false;
            
            // Создаем копию выбранного продукта для редактирования
            EditingProduct = new Product
            {
                Id = SelectedProduct.Id,
                Title = SelectedProduct.Title,
                Description = SelectedProduct.Description,
                Price = SelectedProduct.Price,
                ImageUrl = SelectedProduct.ImageUrl,
                Artist = SelectedProduct.Artist,
                Medium = SelectedProduct.Medium,
                Size = SelectedProduct.Size,
                Year = SelectedProduct.Year,
                InStock = SelectedProduct.InStock,
                CreatedAt = SelectedProduct.CreatedAt
            };
        }
        
        private bool CanDeleteProduct()
        {
            return _authService.IsAdmin && SelectedProduct != null && !IsEditing && !IsAddingNew;
        }
        
        private void DeleteProduct()
        {
            if (!_authService.IsAdmin)
            {
                ErrorMessage = "Доступ запрещен. Требуются права администратора.";
                return;
            }
            
            if (SelectedProduct == null)
            {
                ErrorMessage = "Выберите товар для удаления";
                return;
            }
            
            try
            {
                var result = _productService.DeleteProduct(SelectedProduct.Id);
                
                if (!result)
                {
                    ErrorMessage = "Не удалось удалить товар";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при удалении товара: {ex.Message}";
            }
        }
        
        private bool CanSaveProduct()
        {
            return _authService.IsAdmin && (IsEditing || IsAddingNew) && EditingProduct != null && 
                   !string.IsNullOrWhiteSpace(EditingProduct.Title) &&
                   !string.IsNullOrWhiteSpace(EditingProduct.Artist) &&
                   EditingProduct.Price > 0;
        }
        
        private void SaveProduct()
        {
            if (!_authService.IsAdmin)
            {
                ErrorMessage = "Доступ запрещен. Требуются права администратора.";
                return;
            }
            
            if (EditingProduct == null)
            {
                ErrorMessage = "Нет данных для сохранения";
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("SaveProduct validating input fields");
            
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(EditingProduct.Title))
            {
                ErrorMessage = "Необходимо указать название товара";
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
                System.Diagnostics.Debug.WriteLine("Validation failed: Title is empty");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(EditingProduct.Artist))
            {
                ErrorMessage = "Необходимо указать художника";
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
                System.Diagnostics.Debug.WriteLine("Validation failed: Artist is empty");
                return;
            }
            
            if (EditingProduct.Price <= 0)
            {
                ErrorMessage = "Цена должна быть больше нуля";
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
                System.Diagnostics.Debug.WriteLine("Validation failed: Price <= 0");
                return;
            }
            
            try
            {
                bool result;
                
                if (IsAddingNew)
                {
                    result = _productService.AddProduct(EditingProduct);
                }
                else
                {
                    result = _productService.UpdateProduct(EditingProduct);
                }
                
                if (result)
                {
                    // Сбросить состояние редактирования
                    IsEditing = false;
                    IsAddingNew = false;
                    EditingProduct = null;
                }
                else
                {
                    ErrorMessage = "Не удалось сохранить товар";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при сохранении товара: {ex.Message}";
            }
        }
        
        private void CancelEdit()
        {
            System.Diagnostics.Debug.WriteLine("CancelEdit called");
            
            IsEditing = false;
            IsAddingNew = false;
            EditingProduct = null;
            ErrorMessage = string.Empty;
            
            System.Diagnostics.Debug.WriteLine($"After cancel: IsEditing={IsEditing}, IsAddingNew={IsAddingNew}");
            System.Diagnostics.Debug.WriteLine($"EditingVisibility={EditingVisibility}, NotEditingVisibility={NotEditingVisibility}");
            
            // Обновляем состояние команд
            InvalidateCommands();
        }
        
        private void OnProductsChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }
        
        private void InvalidateCommands()
        {
            // Принудительное обновление состояния команд
            System.Diagnostics.Debug.WriteLine("Invalidating commands...");
            CommandManager.InvalidateRequerySuggested();
        }
        
        public void Dispose()
        {
            // Отписываемся от событий
            _productService.ProductsChanged -= OnProductsChanged;
            
            System.Diagnostics.Debug.WriteLine("AdminPanelViewModel: Disposed and unsubscribed from events");
        }
    }
}