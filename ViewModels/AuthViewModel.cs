using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class AuthViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        
        // Свойства для входа
        private string _loginUsername = string.Empty;
        private string _loginPassword = string.Empty;
        private string _loginError = string.Empty;
        
        // Свойства для регистрации
        private string _registerUsername = string.Empty;
        private string _registerPassword = string.Empty;
        private string _registerConfirmPassword = string.Empty;
        private string _registerEmail = string.Empty;
        private string _registerName = string.Empty;
        private string _registerError = string.Empty;
        
        // Переключение между формами входа и регистрации
        private bool _isLoginView = true;
        
        // Индикатор загрузки
        private bool _isLoading;
        
        public string LoginUsername
        {
            get => _loginUsername;
            set => SetProperty(ref _loginUsername, value);
        }
        
        public string LoginPassword
        {
            get => _loginPassword;
            set => SetProperty(ref _loginPassword, value);
        }
        
        public string LoginError
        {
            get => _loginError;
            set 
            {
                if (SetProperty(ref _loginError, value))
                {
                    OnPropertyChanged(nameof(LoginErrorVisibility));
                }
            }
        }
        
        public string RegisterUsername
        {
            get => _registerUsername;
            set => SetProperty(ref _registerUsername, value);
        }
        
        public string RegisterPassword
        {
            get => _registerPassword;
            set => SetProperty(ref _registerPassword, value);
        }
        
        public string RegisterConfirmPassword
        {
            get => _registerConfirmPassword;
            set => SetProperty(ref _registerConfirmPassword, value);
        }
        
        public string RegisterEmail
        {
            get => _registerEmail;
            set => SetProperty(ref _registerEmail, value);
        }
        
        public string RegisterName
        {
            get => _registerName;
            set => SetProperty(ref _registerName, value);
        }
        
        public string RegisterError
        {
            get => _registerError;
            set 
            {
                if (SetProperty(ref _registerError, value))
                {
                    OnPropertyChanged(nameof(RegisterErrorVisibility));
                }
            }
        }
        
        public bool IsLoginView
        {
            get => _isLoginView;
            set 
            {
                if (SetProperty(ref _isLoginView, value))
                {
                    OnPropertyChanged(nameof(IsRegisterView));
                    OnPropertyChanged(nameof(LoginViewVisibility));
                    OnPropertyChanged(nameof(RegisterViewVisibility));
                    OnPropertyChanged(nameof(LoginTabBorderThickness));
                    OnPropertyChanged(nameof(RegisterTabBorderThickness));
                }
            }
        }
        
        public bool IsRegisterView => !IsLoginView;
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        // Свойства видимости
        public Visibility LoginViewVisibility => IsLoginView ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RegisterViewVisibility => IsRegisterView ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoginErrorVisibility => string.IsNullOrEmpty(LoginError) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility RegisterErrorVisibility => string.IsNullOrEmpty(RegisterError) ? Visibility.Collapsed : Visibility.Visible;
        public Thickness LoginTabBorderThickness => IsLoginView ? new Thickness(2, 0, 0, 0) : new Thickness(0);
        public Thickness RegisterTabBorderThickness => IsRegisterView ? new Thickness(2, 0, 0, 0) : new Thickness(0);
        
        // Команды
        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand SwitchToLoginCommand { get; set; }
        public ICommand SwitchToRegisterCommand { get; set; }
        
        public AuthViewModel(AuthService authService)
        {
            _authService = authService;
            
            // Инициализация команд
            LoginCommand = new RelayCommand(param => Login(), param => CanLogin());
            RegisterCommand = new RelayCommand(param => Register(), param => CanRegister());
            SwitchToLoginCommand = new RelayCommand(param => IsLoginView = true);
            SwitchToRegisterCommand = new RelayCommand(param => IsLoginView = false);
        }
        
        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(LoginUsername) && 
                   !string.IsNullOrWhiteSpace(LoginPassword) &&
                   !IsLoading;
        }
        
        private void Login()
        {
            Debug.WriteLine($"Login attempt with username: {LoginUsername}");
            LoginError = string.Empty;
            IsLoading = true;
            
            try
            {
                bool success = _authService.Login(LoginUsername, LoginPassword);
                Debug.WriteLine($"Login result: {success}, IsAdmin: {_authService.IsAdmin}");
                
                if (!success)
                {
                    LoginError = "Неверное имя пользователя или пароль";
                }
            }
            catch (Exception ex)
            {
                LoginError = $"Ошибка при входе: {ex.Message}";
                Debug.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(RegisterUsername) &&
                   !string.IsNullOrWhiteSpace(RegisterPassword) &&
                   !string.IsNullOrWhiteSpace(RegisterConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(RegisterEmail) &&
                   RegisterPassword == RegisterConfirmPassword &&
                   !IsLoading;
        }
        
        private void Register()
        {
            Debug.WriteLine($"Register attempt with username: {RegisterUsername}, email: {RegisterEmail}");
            RegisterError = string.Empty;
            IsLoading = true;
            
            try
            {
                if (RegisterPassword != RegisterConfirmPassword)
                {
                    RegisterError = "Пароли не совпадают";
                    Debug.WriteLine("Registration error: passwords do not match");
                    return;
                }
                
                bool success = _authService.Register(
                    RegisterUsername, 
                    RegisterPassword, 
                    RegisterEmail, 
                    RegisterName);
                
                Debug.WriteLine($"Registration result: {success}");
                
                if (!success)
                {
                    RegisterError = "Пользователь с таким именем или email уже существует";
                }
                else
                {
                    Debug.WriteLine($"User successfully registered and logged in. IsAdmin: {_authService.IsAdmin}");
                }
            }
            catch (Exception ex)
            {
                RegisterError = $"Ошибка при регистрации: {ex.Message}";
                Debug.WriteLine($"Registration error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}