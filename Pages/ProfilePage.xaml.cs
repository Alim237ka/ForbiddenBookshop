using ForbiddenBookshop.DB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ForbiddenBookshop.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private Users currentUser;
        private Customers customerData;

        public ProfilePage()
        {
            InitializeComponent();
            this.Loaded += ProfilePage_Loaded;
        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Не авторизован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (NavigationService != null)
                    NavigationService.Navigate(new LoginPage());
                else
                    (Application.Current.MainWindow as MainWindow)?.MainFrame.Navigate(new LoginPage());
                return;
            }

            currentUser = DBClass.connect.Users.Find(App.CurrentUser.UserID);
            if (currentUser == null) return;

            customerData = DBClass.connect.Customers.FirstOrDefault(c => c.UserID == currentUser.UserID);

            tbLogin.Text = currentUser.Login;
            tbFullName.Text = currentUser.FullName;

            if (customerData != null)
                tbMadnessLevel.Text = $"{customerData.MadnessLevel}/100";

            var orders = new List<Orders>();
            if (customerData != null)
            {
                orders = DBClass.connect.Orders.Where(o => o.CustomerID == customerData.CustomerID).OrderByDescending(o => o.OrderDate).ToList();
            }
            lvOrders.ItemsSource = orders;
        }

        private void BtnSelectAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Image files|*.jpg;*.png;*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                string avatarsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
                if (!Directory.Exists(avatarsDir)) Directory.CreateDirectory(avatarsDir);

                string fileName = $"{currentUser.UserID}_{Guid.NewGuid()}{System.IO.Path.GetExtension(dialog.FileName)}";
                string destPath = System.IO.Path.Combine(avatarsDir, fileName);
                File.Copy(dialog.FileName, destPath, true);

                imgAvatar.Source = new BitmapImage(new Uri(destPath));
                MessageBox.Show("Аватар обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            currentUser.FullName = tbFullName.Text.Trim();
            if (!string.IsNullOrEmpty(pbPassword.Password))
                currentUser.PasswordHash = pbPassword.Password;

            DBClass.connect.SaveChanges();
            MessageBox.Show("Профиль обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
