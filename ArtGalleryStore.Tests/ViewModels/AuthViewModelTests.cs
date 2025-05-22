using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.ViewModels;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;
using System.Linq;
using Moq;

namespace ArtGalleryStore.Tests.ViewModels
{
    [TestClass]
    public class AuthViewModelTests
    {
        private Mock<AuthService> _mockAuthService;
        private AuthViewModel _viewModel;
        private User _testUser;

        [TestInitialize]
        public void Setup()
        {
            // Создаем тестового пользователя
            _testUser = new User 
            { 
                Id = 1, 
                Username = "testuser", 
                Name = "Test User",
                IsAdmin = false
            };
            
            // Настраиваем мок AuthService для успешной авторизации
            _mockAuthService = new Mock<AuthService>();
            
            // Настраиваем метод Login
            _mockAuthService.Setup(a => a.Login(
                It.Is<string>(u => u == "testuser"), 
                It.Is<string>(p => p == "correctpassword")))
                .Returns(new AuthResult { Success = true, User = _testUser });
                
            _mockAuthService.Setup(a => a.Login(
                It.Is<string>(u => u != "testuser" || u == null), 
                It.IsAny<string>()))
                .Returns(new AuthResult { Success = false, ErrorMessage = "Неверное имя пользователя" });
                
            _mockAuthService.Setup(a => a.Login(
                It.Is<string>(u => u == "testuser"), 
                It.Is<string>(p => p != "correctpassword" || p == null)))
                .Returns(new AuthResult { Success = false, ErrorMessage = "Неверный пароль" });
            
            // Настраиваем метод Register
            _mockAuthService.Setup(a => a.Register(
                It.Is<string>(u => u != "existinguser"), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<bool>()))
                .Returns(new AuthResult { Success = true, User = _testUser });
                
            _mockAuthService.Setup(a => a.Register(
                It.Is<string>(u => u == "existinguser"), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<bool>()))
                .Returns(new AuthResult { Success = false, ErrorMessage = "Пользователь уже существует" });
            
            // Создаем ViewModel
            _viewModel = new AuthViewModel(_mockAuthService.Object);
        }

        [TestMethod]
        public void Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            _viewModel.LoginUsername = "testuser";
            _viewModel.LoginPassword = "correctpassword";
            
            // Act
            _viewModel.LoginCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            _mockAuthService.Verify(a => a.Login("testuser", "correctpassword"), Times.Once);
        }

        [TestMethod]
        public void Login_WithInvalidUsername_ShouldShowError()
        {
            // Arrange
            _viewModel.LoginUsername = "wronguser";
            _viewModel.LoginPassword = "anypassword";
            
            // Act
            _viewModel.LoginCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            _mockAuthService.Verify(a => a.Login("wronguser", "anypassword"), Times.Once);
        }

        [TestMethod]
        public void Login_WithInvalidPassword_ShouldShowError()
        {
            // Arrange
            _viewModel.LoginUsername = "testuser";
            _viewModel.LoginPassword = "wrongpassword";
            
            // Act
            _viewModel.LoginCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            _mockAuthService.Verify(a => a.Login("testuser", "wrongpassword"), Times.Once);
        }

        [TestMethod]
        public void Login_WithEmptyUsername_ShouldShowValidationError()
        {
            // Arrange
            _viewModel.LoginUsername = "";
            _viewModel.LoginPassword = "anypassword";
            
            // Act
            _viewModel.LoginCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            _mockAuthService.Verify(a => a.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Login_WithEmptyPassword_ShouldShowValidationError()
        {
            // Arrange
            _viewModel.LoginUsername = "testuser";
            _viewModel.LoginPassword = "";
            
            // Act
            _viewModel.LoginCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            _mockAuthService.Verify(a => a.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Register_WithValidData_ShouldSucceed()
        {
            // Arrange
            _viewModel.RegisterUsername = "newuser";
            _viewModel.RegisterPassword = "Password123";
            _viewModel.RegisterName = "New User";
            
            // Act
            _viewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
            _mockAuthService.Verify(a => a.Register(
                "newuser", "Password123", "New User", false), Times.Once);
        }

        [TestMethod]
        public void Register_WithExistingUsername_ShouldShowError()
        {
            // Arrange
            _viewModel.RegisterUsername = "existinguser";
            _viewModel.RegisterPassword = "Password123";
            _viewModel.RegisterName = "Existing User";
            
            // Act
            _viewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
            _mockAuthService.Verify(a => a.Register(
                "existinguser", "Password123", "Existing User", false), Times.Once);
        }

        [TestMethod]
        public void Register_WithEmptyUsername_ShouldShowValidationError()
        {
            // Arrange
            _viewModel.RegisterUsername = "";
            _viewModel.RegisterPassword = "Password123";
            _viewModel.RegisterName = "Any Name";
            
            // Act
            _viewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
            _mockAuthService.Verify(a => a.Register(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void Register_WithEmptyPassword_ShouldShowValidationError()
        {
            // Arrange
            _viewModel.RegisterUsername = "newuser";
            _viewModel.RegisterPassword = "";
            _viewModel.RegisterName = "Any Name";
            
            // Act
            _viewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
            _mockAuthService.Verify(a => a.Register(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void Register_WithEmptyName_ShouldShowValidationError()
        {
            // Arrange
            _viewModel.RegisterUsername = "newuser";
            _viewModel.RegisterPassword = "Password123";
            _viewModel.RegisterName = "";
            
            // Act
            _viewModel.RegisterCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
            _mockAuthService.Verify(a => a.Register(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void SwitchToRegister_ShouldShowRegisterForm()
        {
            // Act
            _viewModel.SwitchToRegisterCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_viewModel.IsRegisterMode);
        }

        [TestMethod]
        public void SwitchToLogin_ShouldShowLoginForm()
        {
            // Arrange
            _viewModel.IsRegisterMode = true;
            
            // Act
            _viewModel.SwitchToLoginCommand.Execute(null);
            
            // Assert
            Assert.IsFalse(_viewModel.IsRegisterMode);
        }

        [TestMethod]
        public void ClearErrors_ShouldClearErrorMessages()
        {
            // Arrange
            _viewModel.LoginErrorMessage = "Test error";
            _viewModel.RegisterErrorMessage = "Test error";
            
            // Act
            _viewModel.ClearLoginErrorCommand.Execute(null);
            _viewModel.ClearRegisterErrorCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.LoginErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(_viewModel.RegisterErrorMessage));
        }
    }
}