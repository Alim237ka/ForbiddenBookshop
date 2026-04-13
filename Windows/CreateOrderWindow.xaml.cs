using ForbiddenBookshop.DB;
using ForbiddenBookshop.Pages;
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
    /// Логика взаимодействия для CreateOrderWindow.xaml
    /// </summary>
    public partial class CreateOrderWindow : Window
    {
        private int bookId;
        private Books book;
        private Customers customer;

        public CreateOrderWindow(int bookId)
        {
            InitializeComponent();
            this.bookId = bookId;
            LoadData();
        }

        private void LoadData()
        {
            book = DBClass.connect.Books.Find(bookId);
            customer = DBClass.connect.Customers.FirstOrDefault(c => c.UserID == App.CurrentUser.UserID);

            tbBookTitle.Text = book.Title;
            tbPrice.Text = $"{book.Price:C}";
            UpdateTotal();

            tbQuantity.TextChanged += (s, e) => UpdateTotal();
        }

        private void UpdateTotal()
        {
            if (int.TryParse(tbQuantity.Text, out int qty) && qty > 0)
            {
                decimal total = book.Price * qty;
                tbTotal.Text = $"{total:C}";
            }
            else
            {
                tbTotal.Text = "0.00";
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (book.Stock < qty)
            {
                MessageBox.Show($"Недостаточно книг на складе. В наличии: {book.Stock}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var order = new Orders
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = book.Price * qty,
                Status = "Новый"
            };
            DBClass.connect.Orders.Add(order);
            DBClass.connect.SaveChanges();

            var orderBook = new OrderBooks
            {
                OrderID = order.OrderID,
                BookID = bookId,
                Quantity = qty,
                PriceAtMoment = book.Price
            };
            DBClass.connect.OrderBooks.Add(orderBook);

            book.Stock -= qty;

            var newMadness = (customer.MadnessLevel ?? 0) + (book.CurseLevel ?? 0) * qty;
            if (newMadness > 100) newMadness = 100;
            customer.MadnessLevel = newMadness;

            var log = new Logs
            {
                UserID = App.CurrentUser.UserID,
                ActionType = "Заказ",
                ActionDescription = $"Пользователь {App.CurrentUser.Login} заказал книгу '{book.Title}' в количестве {qty} шт.",
                ActionDate = DateTime.Now
            };
            DBClass.connect.Logs.Add(log);

            DBClass.connect.SaveChanges();

            if (newMadness >= 100)
            {
                var disappearance = new DisappearedReaders
                {
                    CustomerID = customer.CustomerID,
                    BookID = bookId,
                    DisappearanceDate = DateTime.Now,
                    Notes = $"Читатель исчез после прочтения книги '{book.Title}' (уровень безумия достиг 100)"
                };
                DBClass.connect.DisappearedReaders.Add(disappearance);
                DBClass.connect.SaveChanges();

                MessageBox.Show($"⚠️ ВНИМАНИЕ! Читатель {App.CurrentUser.FullName} достиг 100% безумия и исчез! ⚠️",
                    "Исчезновение", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
                App.CurrentUser = null;
                (Application.Current.MainWindow as MainWindow)?.MainFrame.Navigate(new LoginPage());
            }
            else
            {
                MessageBox.Show($"Заказ создан! Ваш уровень безумия: {newMadness}/100", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
