using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ForbiddenBookshop.Windows;
using ForbiddenBookshop.Pages;

namespace ForbiddenBookshop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (App.CurrentUser != null)
                MainFrame.Navigate(new CatalogPage());
            else
                MainFrame.Navigate(new LoginPage());
        }

        private void btnCatalog_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
        private void btnProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID == 1)
                MainFrame.Navigate(new AdminPage());
            else
                MessageBox.Show("Доступ только администратору", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void btnExorcist_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID == 3)
                MainFrame.Navigate(new ExorcistPage());
            else
                MessageBox.Show("Доступ только библиотекарю-экзорцисту", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            MainFrame.Navigate(new LoginPage());
        }
    }
}
