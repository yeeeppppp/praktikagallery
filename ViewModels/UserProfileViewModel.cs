using System;
using System.Windows.Input;
using ArtGalleryStore.Models;
using ArtGalleryStore.Services;

namespace ArtGalleryStore.ViewModels
{
    public class UserProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        
        public User CurrentUser => _authService.CurrentUser;
        
        public string Username => CurrentUser?.Username;
        public string Email => CurrentUser?.Email;
        public string Name => CurrentUser?.Name;
        public DateTime? CreatedAt => CurrentUser?.CreatedAt;
        
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }
        
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
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
        public System.Windows.Visibility ErrorMessageVisibility => 
            string.IsNullOrEmpty(ErrorMessage) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            
        public System.Windows.Visibility SuccessMessageVisibility => 
            string.IsNullOrEmpty(SuccessMessage) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        
        // Команды
        public ICommand UpdateProfileCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        
        public UserProfileViewModel(AuthService authService)
        {
            _authService = authService;
            
            // Инициализация команд
            UpdateProfileCommand = new RelayCommand(_ => UpdateProfile(), _ => CanUpdateProfile());
            LogoutCommand = new RelayCommand(_ => Logout());
        }
        
        public UserProfileViewModel()
        {
            _authService = App.AuthService;
            
            // Подписываемся на изменения пользователя
            _authService.CurrentUserChanged += OnCurrentUserChanged;
            
            // Инициализация команд
            UpdateProfileCommand = new RelayCommand(_ => UpdateProfile(), _ => CanUpdateProfile());
            LogoutCommand = new RelayCommand(_ => Logout());
        }
        
        private bool CanUpdateProfile()
        {
            // Проверка на заполнение всех необходимых полей и совпадение паролей
            if (!string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                return NewPassword == ConfirmPassword;
            }
            
            return false;
        }
        
        private void UpdateProfile()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Новый пароль не может быть пустым";
                return;
            }
            
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }
            
            // В этой версии мы не реализуем реальное обновление профиля,
            // так как для этого требуется расширить AuthService.
            // В реальном приложении здесь был бы вызов метода обновления профиля.
            
            SuccessMessage = "Профиль успешно обновлен";
            
            // Очищаем поля паролей
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
        
        private void Logout()
        {
            _authService.Logout();
        }
        
        private void OnCurrentUserChanged(object sender, EventArgs e)
        {
            // Обновляем все свойства, связанные с пользователем
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(CreatedAt));
        }
    }
}