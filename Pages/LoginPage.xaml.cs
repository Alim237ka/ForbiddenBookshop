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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = tbLogin.Text.Trim();
                string pass = pbPassword.Password;

                var user = DBClass.connect.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == pass);
                if (user == null)
                {
                    tbError.Text = "Неверный логин или пароль";
                    return;
                }

                App.CurrentUser = user;

                var log = new Logs
                {
                    UserID = user.UserID,
                    ActionType = "Вход",
                    ActionDescription = $"Пользователь {user.Login} вошел в систему",
                    ActionDate = DateTime.Now
                };
                DBClass.connect.Logs.Add(log);
                DBClass.connect.SaveChanges();

                NavigationService.Navigate(new CatalogPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Что-то пошло не так\n{ex.Message}");
            }
        }

        private void BtnReg_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new RegisterPage());
    }
}
