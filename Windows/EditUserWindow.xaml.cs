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
using System.Windows.Shapes;

namespace ForbiddenBookshop.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        private Users user;
        private bool isNew;

        public EditUserWindow(Users existing = null)
        {
            InitializeComponent();
            if (existing == null)
            {
                isNew = true;
                user = new Users();
            }
            else
            {
                isNew = false;
                user = existing;
                tbLogin.Text = user.Login;
                tbFullName.Text = user.FullName;

                foreach (ComboBoxItem item in cmbRole.Items)
                {
                    if (item.Tag.ToString() == user.RoleID.ToString())
                    {
                        cmbRole.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            user.Login = tbLogin.Text.Trim();
            user.FullName = tbFullName.Text.Trim();

            if (cmbRole.SelectedItem != null)
                user.RoleID = int.Parse(((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString());

            if (!string.IsNullOrEmpty(pbPassword.Password))
                user.PasswordHash = pbPassword.Password;

            if (isNew)
            {
                DBClass.connect.Users.Add(user);
                DBClass.connect.SaveChanges();

                if (user.RoleID == 2)
                {
                    var newCustomer = new Customers
                    {
                        UserID = user.UserID,
                        MadnessLevel = 0,
                        RegistrationDate = DateTime.Now
                    };
                    DBClass.connect.Customers.Add(newCustomer);
                }
                else if (user.RoleID == 3)
                {
                    var newKeeper = new Keepers
                    {
                        UserID = user.UserID,
                        ExperienceYears = 0,
                        ExorcismCertified = false
                    };
                    DBClass.connect.Keepers.Add(newKeeper);
                }
            }

            var log = new Logs
            {
                UserID = App.CurrentUser.UserID,
                ActionType = isNew ? "Добавление пользователя" : "Редактирование пользователя",
                ActionDescription = isNew ? $"Добавлен пользователь '{user.Login}'" : $"Отредактирован пользователь '{user.Login}'",
                ActionDate = DateTime.Now
            };
            DBClass.connect.Logs.Add(log);

            DBClass.connect.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
