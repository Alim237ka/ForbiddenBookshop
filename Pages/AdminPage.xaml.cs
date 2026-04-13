using ForbiddenBookshop.DB;
using ForbiddenBookshop.Windows;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            if (App.CurrentUser?.RoleID != 1)
            {
                MessageBox.Show("Нет прав доступа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }
            RefreshUsers();
            RefreshLogs();
            RefreshDisappeared();
        }

        private void RefreshUsers() => lvUsers.ItemsSource = DBClass.connect.Users.ToList();
        private void RefreshLogs() => lvLogs.ItemsSource = DBClass.connect.Logs.OrderByDescending(l => l.ActionDate).ToList();
        private void RefreshDisappeared() => lvDisappeared.ItemsSource = DBClass.connect.DisappearedReaders.ToList();

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            new EditUserWindow().ShowDialog();
            RefreshUsers();
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var user = DBClass.connect.Users.Find(id);
            new EditUserWindow(user).ShowDialog();
            RefreshUsers();
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var user = DBClass.connect.Users.Find(id);
            if (user == null) return;

            if (MessageBox.Show($"Удалить пользователя {user.Login}? Все его заказы и логи также будут удалены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            var orders = DBClass.connect.Orders.Where(o => o.Customers.UserID == id).ToList();
            foreach (var order in orders)
            {
                var orderBooks = DBClass.connect.OrderBooks.Where(ob => ob.OrderID == order.OrderID).ToList();
                foreach (var ob in orderBooks)
                {
                    DBClass.connect.OrderBooks.Remove(ob);
                }
            }

            foreach (var order in orders)
            {
                DBClass.connect.Orders.Remove(order);
            }

            var logs = DBClass.connect.Logs.Where(l => l.UserID == id).ToList();
            foreach (var log in logs)
            {
                DBClass.connect.Logs.Remove(log);
            }

            var customer = DBClass.connect.Customers.FirstOrDefault(c => c.UserID == id);
            if (customer != null)
            {
                var disappeared = DBClass.connect.DisappearedReaders.Where(d => d.CustomerID == customer.CustomerID).ToList();
                foreach (var d in disappeared)
                {
                    DBClass.connect.DisappearedReaders.Remove(d);
                }
                DBClass.connect.Customers.Remove(customer);
            }

            var keeper = DBClass.connect.Keepers.FirstOrDefault(k => k.UserID == id);
            if (keeper != null)
            {
                DBClass.connect.Keepers.Remove(keeper);
            }

            DBClass.connect.Users.Remove(user);

            DBClass.connect.SaveChanges();

            RefreshUsers();
            MessageBox.Show("Пользователь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
