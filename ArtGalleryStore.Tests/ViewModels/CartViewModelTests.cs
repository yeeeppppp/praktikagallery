using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.ViewModels;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using System.Collections.ObjectModel;
using System.Windows;

namespace ArtGalleryStore.Tests.ViewModels
{
    [TestClass]
    public class CartViewModelTests
    {
        private Mock<CartService> _mockCartService;
        private Mock<AuthService> _mockAuthService;
        private CartViewModel _viewModel;
        private User _testUser;
        private List<CartItem> _testCartItems;
        private Product _testProduct;

        [TestInitialize]
        public void Setup()
        {
            // Создаем тестового пользователя
            _testUser = new User { Id = 1, Username = "testuser", Name = "Test User" };
            
            // Создаем тестовый продукт
            _testProduct = new Product 
            { 
                Id = 1, 
                Title = "Тестовая картина", 
                Artist = "Тестовый художник", 
                Price = 15000, 
                InStock = true 
            };
            
            // Создаем тестовые элементы корзины
            _testCartItems = new List<CartItem>
            {
                new CartItem 
                { 
                    Id = 1, 
                    UserId = _testUser.Id, 
                    ProductId = _testProduct.Id, 
                    Product = _testProduct, 
                    Quantity = 2 
                }
            };
            
            // Создаем тестовую корзину
            var testCart = new Cart
            {
                UserId = _testUser.Id,
                Items = _testCartItems
            };
            
            // Настраиваем мок CartService
            _mockCartService = new Mock<CartService>();
            _mockCartService.Setup(c => c.GetCart(_testUser.Id)).Returns(testCart);
            _mockCartService.Setup(c => c.UpdateCartItemQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            _mockCartService.Setup(c => c.RemoveFromCart(
                It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            _mockCartService.Setup(c => c.ClearCart(It.IsAny<int>())).Returns(true);
            
            // Настраиваем мок AuthService
            _mockAuthService = new Mock<AuthService>();
            _mockAuthService.Setup(a => a.IsLoggedIn).Returns(true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(_testUser);
            
            // Создаем ViewModel
            _viewModel = new CartViewModel(_mockAuthService.Object, _mockCartService.Object);
        }

        [TestMethod]
        public void LoadCart_LoggedInUser_ShouldPopulateCartItems()
        {
            // Act
            _viewModel.LoadCart();
            
            // Assert
            Assert.AreEqual(_testCartItems.Count, _viewModel.CartItems.Count);
            Assert.AreEqual(_testProduct.Price * _testCartItems[0].Quantity, _viewModel.Total);
            Assert.IsFalse(_viewModel.IsCartEmpty);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.EmptyCartVisibility);
            Assert.AreEqual(Visibility.Visible, _viewModel.CartItemsVisibility);
        }

        [TestMethod]
        public void LoadCart_NotLoggedIn_ShouldClearCart()
        {
            // Arrange
            _mockAuthService.Setup(a => a.IsLoggedIn).Returns(false);
            _mockAuthService.Setup(a => a.CurrentUser).Returns((User)null);
            
            // Act
            _viewModel.LoadCart();
            
            // Assert
            Assert.AreEqual(0, _viewModel.CartItems.Count);
            Assert.AreEqual(0, _viewModel.Total);
            Assert.IsTrue(_viewModel.IsCartEmpty);
            Assert.AreEqual(Visibility.Visible, _viewModel.EmptyCartVisibility);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.CartItemsVisibility);
        }

        [TestMethod]
        public void IncreaseQuantity_ShouldCallCartService()
        {
            // Arrange
            var cartItem = _testCartItems[0];
            int newQuantity = cartItem.Quantity + 1;
            
            // Act
            _viewModel.IncreaseQuantityCommand.Execute(cartItem);
            
            // Assert
            _mockCartService.Verify(c => 
                c.UpdateCartItemQuantity(_testUser.Id, cartItem.Id, newQuantity), Times.Once);
        }

        [TestMethod]
        public void DecreaseQuantity_QuantityGreaterThanOne_ShouldDecreaseQuantity()
        {
            // Arrange
            var cartItem = _testCartItems[0];
            cartItem.Quantity = 2; // Ensure quantity is > 1
            int newQuantity = cartItem.Quantity - 1;
            
            // Act
            _viewModel.DecreaseQuantityCommand.Execute(cartItem);
            
            // Assert
            _mockCartService.Verify(c => 
                c.UpdateCartItemQuantity(_testUser.Id, cartItem.Id, newQuantity), Times.Once);
        }

        [TestMethod]
        public void DecreaseQuantity_QuantityIsOne_ShouldRemoveItem()
        {
            // Arrange
            var cartItem = _testCartItems[0];
            cartItem.Quantity = 1; // Set quantity to 1
            
            // Act
            _viewModel.DecreaseQuantityCommand.Execute(cartItem);
            
            // Assert
            _mockCartService.Verify(c => 
                c.RemoveFromCart(_testUser.Id, cartItem.Id), Times.Once);
        }

        [TestMethod]
        public void RemoveItem_ShouldCallCartService()
        {
            // Arrange
            var cartItem = _testCartItems[0];
            
            // Act
            _viewModel.RemoveItemCommand.Execute(cartItem);
            
            // Assert
            _mockCartService.Verify(c => 
                c.RemoveFromCart(_testUser.Id, cartItem.Id), Times.Once);
        }

        [TestMethod]
        public void ClearCart_ShouldCallCartService()
        {
            // Act
            _viewModel.ClearCartCommand.Execute(null);
            
            // Assert
            _mockCartService.Verify(c => 
                c.ClearCart(_testUser.Id), Times.Once);
        }

        [TestMethod]
        public void Checkout_EmptyCart_ShouldShowError()
        {
            // Arrange
            _viewModel.CartItems.Clear();
            
            // Act
            _viewModel.CheckoutCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.ErrorMessage));
            Assert.IsFalse(_viewModel.IsCheckingOut);
        }

        [TestMethod]
        public void Checkout_NonEmptyCart_ShouldEnterCheckoutMode()
        {
            // Act
            _viewModel.CheckoutCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_viewModel.IsCheckingOut);
            Assert.AreEqual(Visibility.Visible, _viewModel.CheckoutFormVisibility);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.NotCheckoutFormVisibility);
        }

        [TestMethod]
        public void PlaceOrder_EmptyShippingAddress_ShouldShowError()
        {
            // Arrange
            _viewModel.IsCheckingOut = true;
            _viewModel.ShippingAddress = string.Empty;
            
            // Act
            _viewModel.PlaceOrderCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.ErrorMessage));
            Assert.IsFalse(_viewModel.OrderConfirmed);
        }

        [TestMethod]
        public void PlaceOrder_ValidData_ShouldCreateOrder()
        {
            // Arrange
            _viewModel.IsCheckingOut = true;
            _viewModel.ShippingAddress = "Test Address, 123";
            
            // Act
            _viewModel.PlaceOrderCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_viewModel.OrderConfirmed);
            Assert.AreEqual(Visibility.Visible, _viewModel.OrderConfirmationVisibility);
            Assert.IsNotNull(_viewModel.CompletedOrder);
            Assert.AreEqual(_testUser.Id, _viewModel.CompletedOrder.UserId);
            Assert.AreEqual(_viewModel.ShippingAddress, _viewModel.CompletedOrder.ShippingAddress);
        }

        [TestMethod]
        public void ContinueShopping_ShouldResetState()
        {
            // Arrange
            _viewModel.IsCheckingOut = true;
            _viewModel.OrderConfirmed = true;
            _viewModel.ShippingAddress = "Test Address";
            _viewModel.CompletedOrder = new Order();
            _viewModel.ErrorMessage = "Test error";
            
            // Act
            _viewModel.ContinueShoppingCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(_viewModel.IsCheckingOut);
            Assert.IsFalse(_viewModel.OrderConfirmed);
            Assert.AreEqual(string.Empty, _viewModel.ShippingAddress);
            Assert.IsNull(_viewModel.CompletedOrder);
            Assert.AreEqual(string.Empty, _viewModel.ErrorMessage);
        }

        [TestMethod]
        public void CanPlaceOrder_ValidConditions_ShouldReturnTrue()
        {
            // Arrange
            _viewModel.IsCheckingOut = true;
            _viewModel.ShippingAddress = "Test Address";
            
            // Act & Assert
            Assert.IsTrue(_viewModel.PlaceOrderCommand.CanExecute(null));
        }

        [TestMethod]
        public void CanPlaceOrder_EmptyAddress_ShouldReturnFalse()
        {
            // Arrange
            _viewModel.IsCheckingOut = true;
            _viewModel.ShippingAddress = string.Empty;
            
            // Act & Assert
            Assert.IsFalse(_viewModel.PlaceOrderCommand.CanExecute(null));
        }

        [TestMethod]
        public void CanPlaceOrder_NotInCheckoutMode_ShouldReturnFalse()
        {
            // Arrange
            _viewModel.IsCheckingOut = false;
            _viewModel.ShippingAddress = "Test Address";
            
            // Act & Assert
            Assert.IsFalse(_viewModel.PlaceOrderCommand.CanExecute(null));
        }
    }
}