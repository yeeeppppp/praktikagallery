using System;
using System.Collections.Generic;
using System.Linq;
using ArtGalleryStore.Models;

namespace ArtGalleryStore.Services
{
    public class CartService
    {
        private readonly ProductService _productService;
        private readonly Dictionary<int, Cart> _userCarts = new Dictionary<int, Cart>();
        
        // Для отладки
        public Dictionary<int, Cart> UserCarts => _userCarts;

        public event EventHandler CartChanged;
        
        public CartService()
        {
            _productService = App.ProductService;
            System.Diagnostics.Debug.WriteLine("CartService initialized");
        }
        
        public Cart GetCart(int userId)
        {
            System.Diagnostics.Debug.WriteLine($"GetCart вызван для userId={userId}");
            
            // Убедимся, что словарь корзин инициализирован
            if (_userCarts == null)
            {
                System.Diagnostics.Debug.WriteLine("!!! КРИТИЧЕСКАЯ ОШИБКА: _userCarts == null, инициализируем словарь");
                _userCarts = new Dictionary<int, Cart>();
            }
            
            if (!_userCarts.ContainsKey(userId))
            {
                System.Diagnostics.Debug.WriteLine($"Creating new cart for user {userId}");
                var newCart = new Cart { UserId = userId };
                
                // Дополнительная проверка на инициализацию списка Items
                if (newCart.Items == null)
                {
                    System.Diagnostics.Debug.WriteLine("!!! Инициализируем список Items для новой корзины");
                    newCart.Items = new List<CartItem>();
                }
                
                _userCarts[userId] = newCart;
                System.Diagnostics.Debug.WriteLine($"Новая корзина создана. UserId={userId}, Items.Count={newCart.Items.Count}");
            }
            else
            {
                var existingCart = _userCarts[userId];
                System.Diagnostics.Debug.WriteLine($"Found existing cart for user {userId} with {existingCart.Items.Count} items");
                
                // Дополнительная проверка на инициализацию списка Items
                if (existingCart.Items == null)
                {
                    System.Diagnostics.Debug.WriteLine("!!! КРИТИЧЕСКАЯ ОШИБКА: existingCart.Items == null, инициализируем список");
                    existingCart.Items = new List<CartItem>();
                }
            }
            
            return _userCarts[userId];
        }
        
        public bool AddToCart(int userId, int productId, int quantity = 1)
        {
            System.Diagnostics.Debug.WriteLine($"AddToCart: UserId={userId}, ProductId={productId}, Quantity={quantity}");
            
            System.Diagnostics.Debug.WriteLine($"CartService.AddToCart: попытка получить продукт ID={productId}");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                System.Diagnostics.Debug.WriteLine($"AddToCart failed: Product is null. ProductId={productId}");
                return false;
            }
            
            if (!product.InStock)
            {
                System.Diagnostics.Debug.WriteLine($"AddToCart failed: Product out of stock. ProductId={productId}");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"Продукт найден: {product.Title}, Цена: {product.Price}, InStock: {product.InStock}");
                
            System.Diagnostics.Debug.WriteLine($"Получаем корзину для пользователя {userId}");
            
            // Проверяем наличие корзины для пользователя
            if (!_userCarts.ContainsKey(userId))
            {
                System.Diagnostics.Debug.WriteLine($"!!! Корзина для пользователя {userId} не существует, создаем новую");
                _userCarts[userId] = new Cart { UserId = userId };
            }
            else 
            {
                System.Diagnostics.Debug.WriteLine($"Корзина для пользователя {userId} найдена, элементов: {_userCarts[userId].Items.Count}");
            }
            
            var cart = _userCarts[userId];
            System.Diagnostics.Debug.WriteLine($"Current cart for user {userId} has {cart.Items.Count} items");
            
            // Дополнительная проверка на инициализацию списка Items
            if (cart.Items == null)
            {
                System.Diagnostics.Debug.WriteLine("!!! КРИТИЧЕСКАЯ ОШИБКА: cart.Items == null, инициализируем список");
                cart.Items = new List<CartItem>();
            }
            
            // Вывод всех элементов корзины до добавления
            if (cart.Items.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Текущие элементы корзины:");
                foreach (var item in cart.Items)
                {
                    System.Diagnostics.Debug.WriteLine($"  ID={item.Id}, ProductID={item.ProductId}, " +
                                                     $"Product.Title={item.Product?.Title ?? "null"}, " +
                                                     $"Quantity={item.Quantity}");
                }
            }
            
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                System.Diagnostics.Debug.WriteLine($"Updating existing cart item: Id={existingItem.Id}, Old quantity={existingItem.Quantity}, New quantity={existingItem.Quantity + quantity}");
                existingItem.Quantity += quantity;
            }
            else
            {
                var itemId = cart.Items.Count > 0 ? cart.Items.Max(i => i.Id) + 1 : 1;
                System.Diagnostics.Debug.WriteLine($"Creating new cart item: Id={itemId}, ProductId={productId}");
                
                var newItem = new CartItem
                {
                    Id = itemId,
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Product = product,
                    AddedAt = DateTime.Now
                };
                
                cart.Items.Add(newItem);
            }
            
            System.Diagnostics.Debug.WriteLine($"Cart updated successfully. Total items: {cart.Items.Count}, Total quantity: {cart.Items.Sum(i => i.Quantity)}");
            
            // Печатаем обновленное содержимое корзины
            System.Diagnostics.Debug.WriteLine("Обновленные элементы корзины:");
            foreach (var item in cart.Items)
            {
                System.Diagnostics.Debug.WriteLine($"  ID={item.Id}, ProductID={item.ProductId}, " +
                                                 $"Product.Title={item.Product?.Title ?? "null"}, " +
                                                 $"Quantity={item.Quantity}");
            }
            
            System.Diagnostics.Debug.WriteLine($"Состояние всех корзин после добавления товара:");
            foreach (var kvp in _userCarts)
            {
                System.Diagnostics.Debug.WriteLine($"  userId={kvp.Key}, itemCount={kvp.Value.Items.Count}");
            }
            
            // Проверка, есть ли подписчики на событие перед его вызовом
            if (CartChanged != null)
            {
                System.Diagnostics.Debug.WriteLine($"Вызов события CartChanged. Число подписчиков: {CartChanged.GetInvocationList().Length}");
                CartChanged.Invoke(this, EventArgs.Empty);
                System.Diagnostics.Debug.WriteLine("Событие CartChanged было вызвано");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Нет подписчиков на событие CartChanged!");
            }
            
            return true;
        }
        
        public bool UpdateCartItemQuantity(int userId, int itemId, int quantity)
        {
            if (quantity <= 0)
                return RemoveFromCart(userId, itemId);
                
            var cart = GetCart(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
                return false;
                
            item.Quantity = quantity;
            
            // Проверка, есть ли подписчики на событие перед его вызовом
            if (CartChanged != null)
            {
                System.Diagnostics.Debug.WriteLine($"Вызов события CartChanged (UpdateCartItemQuantity). Число подписчиков: {CartChanged.GetInvocationList().Length}");
                CartChanged.Invoke(this, EventArgs.Empty);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Нет подписчиков на событие CartChanged (UpdateCartItemQuantity)!");
            }
            
            return true;
        }
        
        public bool RemoveFromCart(int userId, int itemId)
        {
            var cart = GetCart(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
                return false;
                
            cart.Items.Remove(item);
            
            // Проверка, есть ли подписчики на событие перед его вызовом
            if (CartChanged != null)
            {
                System.Diagnostics.Debug.WriteLine($"Вызов события CartChanged (RemoveFromCart). Число подписчиков: {CartChanged.GetInvocationList().Length}");
                CartChanged.Invoke(this, EventArgs.Empty);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Нет подписчиков на событие CartChanged (RemoveFromCart)!");
            }
            
            return true;
        }
        
        public bool ClearCart(int userId)
        {
            var cart = GetCart(userId);
            cart.Items.Clear();
            
            // Проверка, есть ли подписчики на событие перед его вызовом
            if (CartChanged != null)
            {
                System.Diagnostics.Debug.WriteLine($"Вызов события CartChanged (ClearCart). Число подписчиков: {CartChanged.GetInvocationList().Length}");
                CartChanged.Invoke(this, EventArgs.Empty);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Нет подписчиков на событие CartChanged (ClearCart)!");
            }
            
            return true;
        }
        
        public Order CreateOrder(int userId, string shippingAddress)
        {
            var cart = GetCart(userId);
            
            if (cart.IsEmpty)
                return null;
                
            var order = new Order
            {
                Id = new Random().Next(1000, 9999), // В реальном приложении ID должен генерироваться базой данных
                UserId = userId,
                Total = cart.Total,
                ShippingAddress = shippingAddress,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                Items = cart.Items.Select(item => new OrderItem
                {
                    Id = new Random().Next(10000, 99999), // В реальном приложении ID должен генерироваться базой данных
                    ProductId = item.ProductId,
                    ProductTitle = item.Product.Title,
                    ProductPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    Artist = item.Product.Artist,
                    Medium = item.Product.Medium,
                    Size = item.Product.Size,
                    Product = item.Product
                }).ToList()
            };
            
            // Очищаем корзину после создания заказа
            ClearCart(userId);
            
            return order;
        }
    }
}