using ForbiddenBookshop.DB;
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

namespace ForbiddenBookshop.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = tbLogin.Text.Trim();
                string pass = pbPassword.Password;
                string confirm = pbPasswordConfirm.Password;
                string fullName = tbFullName.Text.Trim();

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(fullName))
                {
                    tbError.Text = "Все поля обязательны";
                    return;
                }
                if (pass != confirm)
                {
                    tbError.Text = "Пароли не совпадают";
                    return;
                }
                if (DBClass.connect.Users.Any(u => u.Login == login))
                {
                    tbError.Text = "Логин уже занят";
                    return;
                }

                var newUser = new Users
                {
                    Login = login,
                    PasswordHash = pass,
                    FullName = fullName,
                    RoleID = 2
                };
                DBClass.connect.Users.Add(newUser);
                DBClass.connect.SaveChanges();

                var newCustomer = new Customers
                {
                    UserID = newUser.UserID,
                    MadnessLevel = 0,
                    RegistrationDate = DateTime.Now,
                    LastOrderDate = null
                };
                DBClass.connect.Customers.Add(newCustomer);
                DBClass.connect.SaveChanges();

                App.CurrentUser = newUser;
                MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new CatalogPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Что-то пошло не так\n{ex.Message}");
            }
        }
    }
}
