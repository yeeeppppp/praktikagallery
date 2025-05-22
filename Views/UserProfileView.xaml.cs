using System.Windows;
using System.Windows.Controls;
using ArtGalleryStore.ViewModels;

namespace ArtGalleryStore.Views
{
    public partial class UserProfileView : UserControl
    {
        public UserProfileView()
        {
            InitializeComponent();
            
            // Добавляем обработчик события загрузки
            this.Loaded += UserProfileView_Loaded;
        }
        
        private void UserProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserProfileViewModel viewModel)
            {
                // Связываем введенные пароли в PasswordBox с командой обновления профиля
                // Так как WPF не поддерживает прямую привязку к PasswordBox.Password
                if (viewModel.UpdateProfileCommand != null)
                {
                    var originalCommand = viewModel.UpdateProfileCommand;
                    viewModel.UpdateProfileCommand = new RelayCommand(
                        param => 
                        {
                            viewModel.NewPassword = NewPasswordBox.Password;
                            viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
                            originalCommand.Execute(param);
                            
                            // Если успешно обновлено - очищаем поля
                            if (string.IsNullOrEmpty(viewModel.ErrorMessage) && !string.IsNullOrEmpty(viewModel.SuccessMessage))
                            {
                                NewPasswordBox.Clear();
                                ConfirmPasswordBox.Clear();
                            }
                        },
                        param => originalCommand.CanExecute(param)
                    );
                }
            }
        }
    }
}