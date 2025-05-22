using System.Windows;
using System.Windows.Controls;
using ArtGalleryStore.ViewModels;

namespace ArtGalleryStore.Views
{
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
            
            // Добавляем обработчики событий для связывания PasswordBox с ViewModel
            this.Loaded += AuthView_Loaded;
        }
        
        private void AuthView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel viewModel)
            {
                // Подписываемся на изменение режима (login/register)
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(AuthViewModel.IsLoginView))
                    {
                        ClearPasswordBoxes();
                    }
                };
                
                // Добавляем обработчики для кнопок, чтобы получить пароль из PasswordBox
                if (viewModel.LoginCommand != null)
                {
                    var originalLoginCommand = viewModel.LoginCommand;
                    viewModel.LoginCommand = new RelayCommand(
                        param => 
                        {
                            viewModel.LoginPassword = LoginPasswordBox.Password;
                            originalLoginCommand.Execute(param);
                        },
                        param => originalLoginCommand.CanExecute(param)
                    );
                }
                
                if (viewModel.RegisterCommand != null)
                {
                    var originalRegisterCommand = viewModel.RegisterCommand;
                    viewModel.RegisterCommand = new RelayCommand(
                        param => 
                        {
                            viewModel.RegisterPassword = RegisterPasswordBox.Password;
                            viewModel.RegisterConfirmPassword = RegisterConfirmPasswordBox.Password;
                            originalRegisterCommand.Execute(param);
                        },
                        param => originalRegisterCommand.CanExecute(param)
                    );
                }
            }
        }
        
        private void ClearPasswordBoxes()
        {
            LoginPasswordBox.Password = string.Empty;
            RegisterPasswordBox.Password = string.Empty;
            RegisterConfirmPasswordBox.Password = string.Empty;
        }
    }
}