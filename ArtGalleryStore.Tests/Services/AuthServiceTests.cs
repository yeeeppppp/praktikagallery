using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;

namespace ArtGalleryStore.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _authService = new AuthService();
        }

        [TestMethod]
        public void RegisterUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            string username = "testuser";
            string password = "Password123!";
            string name = "Test User";
            bool isAdmin = false;

            // Act
            var result = _authService.Register(username, password, name, isAdmin);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.User);
            Assert.AreEqual(username, result.User.Username);
            Assert.AreEqual(name, result.User.Name);
            Assert.AreEqual(isAdmin, result.User.IsAdmin);
        }

        [TestMethod]
        public void RegisterUser_WithDuplicateUsername_ShouldFail()
        {
            // Arrange
            string username = "duplicateuser";
            string password = "Password123!";
            string name = "Duplicate User";

            // First registration should succeed
            var firstResult = _authService.Register(username, password, name, false);
            Assert.IsTrue(firstResult.Success, "First registration should succeed");

            // Act - Try to register with the same username
            var result = _authService.Register(username, "DifferentPassword123!", "Another Name", false);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("уже существует"));
        }

        [TestMethod]
        public void Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            string username = "loginuser";
            string password = "Password123!";
            string name = "Login User";
            
            // Register a test user
            var registerResult = _authService.Register(username, password, name, false);
            Assert.IsTrue(registerResult.Success, "Registration should succeed for this test");

            // Act
            var result = _authService.Login(username, password);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.User);
            Assert.AreEqual(username, result.User.Username);
            Assert.IsTrue(_authService.IsLoggedIn);
            Assert.IsNotNull(_authService.CurrentUser);
        }

        [TestMethod]
        public void Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            string username = "invaliduser";
            string password = "Password123!";
            string name = "Invalid User";
            
            // Register a test user
            var registerResult = _authService.Register(username, password, name, false);
            Assert.IsTrue(registerResult.Success, "Registration should succeed for this test");

            // Act
            var result = _authService.Login(username, "WrongPassword123!");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsFalse(_authService.IsLoggedIn);
            Assert.IsNull(_authService.CurrentUser);
        }

        [TestMethod]
        public void Logout_AfterLogin_ShouldClearCurrentUser()
        {
            // Arrange
            string username = "logoutuser";
            string password = "Password123!";
            
            // Register and login a test user
            _authService.Register(username, password, "Logout User", false);
            var loginResult = _authService.Login(username, password);
            Assert.IsTrue(loginResult.Success, "Login should succeed for this test");
            Assert.IsTrue(_authService.IsLoggedIn);

            // Act
            _authService.Logout();

            // Assert
            Assert.IsFalse(_authService.IsLoggedIn);
            Assert.IsNull(_authService.CurrentUser);
        }

        [TestMethod]
        public void AdminFlag_ShouldReflectUserRole()
        {
            // Arrange
            string adminUsername = "adminuser";
            string regularUsername = "regularuser";
            string password = "Password123!";
            
            // Register admin and regular users
            _authService.Register(adminUsername, password, "Admin User", true);
            _authService.Register(regularUsername, password, "Regular User", false);

            // Act & Assert for admin user
            var adminLoginResult = _authService.Login(adminUsername, password);
            Assert.IsTrue(adminLoginResult.Success, "Admin login should succeed");
            Assert.IsTrue(_authService.IsAdmin, "IsAdmin should be true for admin user");

            // Act & Assert for regular user
            _authService.Logout();
            var regularLoginResult = _authService.Login(regularUsername, password);
            Assert.IsTrue(regularLoginResult.Success, "Regular user login should succeed");
            Assert.IsFalse(_authService.IsAdmin, "IsAdmin should be false for regular user");
        }
    }
}