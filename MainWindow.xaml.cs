using System.Windows;
using ArtGalleryStore.ViewModels;

namespace ArtGalleryStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Используем MainViewModel из App.xaml.cs
            if (App.MainViewModel != null)
            {
                DataContext = App.MainViewModel;
            }
            else
            {
                DataContext = new MainViewModel();
            }
        }
    }
}