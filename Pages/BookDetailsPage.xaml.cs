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
    /// Логика взаимодействия для BookDetailsPage.xaml
    /// </summary>
    public partial class BookDetailsPage : Page
    {
        private int bookId;

        public BookDetailsPage(int id)
        {
            InitializeComponent();
            bookId = id;
            LoadData();
        }

        private void LoadData()
        {
            var book = DBClass.connect.Books.Find(bookId);
            if (book == null) return;

            tbTitle.Text = book.Title;
            tbAuthor.Text = $"Автор: {book.Author ?? "Неизвестен"}";
            tbLanguage.Text = $"Язык: {book.Language ?? "Не указан"}";
            tbCurseLevel.Text = $"⚠️ Уровень проклятия: {book.CurseLevel}/10 ⚠️";
            tbPrice.Text = $"Цена: {book.Price:C}";
            tbStock.Text = $"В наличии: {book.Stock} шт.";
            tbDescription.Text = book.Description ?? "Нет описания";

            var orderBooks = DBClass.connect.OrderBooks.Where(o => o.BookID == bookId).ToList();
            lvOrders.ItemsSource = orderBooks;

            var disappeared = DBClass.connect.DisappearedReaders.Where(d => d.BookID == bookId).ToList();
            lvDisappeared.ItemsSource = disappeared;
        }
    }
}
