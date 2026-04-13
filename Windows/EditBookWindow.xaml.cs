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
    /// Логика взаимодействия для EditBookWindow.xaml
    /// </summary>
    public partial class EditBookWindow : Window
    {
        private Books book;
        private bool isNew;

        public EditBookWindow(Books existingBook = null)
        {
            InitializeComponent();
            if (existingBook == null)
            {
                isNew = true;
                book = new Books();
            }
            else
            {
                isNew = false;
                book = existingBook;
                LoadBookData();
            }
        }

        private void LoadBookData()
        {
            tbTitle.Text = book.Title;
            tbAuthor.Text = book.Author;

            if (book.CurseLevel.HasValue)
            {
                string level = book.CurseLevel.Value.ToString();
                foreach (ComboBoxItem item in cmbCurseLevel.Items)
                    if (item.Content.ToString() == level)
                        cmbCurseLevel.SelectedItem = item;
            }

            if (!string.IsNullOrEmpty(book.Language))
            {
                foreach (ComboBoxItem item in cmbLanguage.Items)
                    if (item.Content.ToString() == book.Language)
                        cmbLanguage.SelectedItem = item;
                if (cmbLanguage.SelectedItem == null)
                    cmbLanguage.Text = book.Language;
            }

            tbPrice.Text = book.Price.ToString();
            tbStock.Text = book.Stock.ToString();
            tbDescription.Text = book.Description;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            book.Title = tbTitle.Text.Trim();
            book.Author = tbAuthor.Text.Trim();

            if (cmbCurseLevel.SelectedItem != null)
                book.CurseLevel = int.Parse((cmbCurseLevel.SelectedItem as ComboBoxItem).Content.ToString());

            book.Language = cmbLanguage.Text.Trim();

            if (decimal.TryParse(tbPrice.Text, out decimal price))
                book.Price = price;

            if (int.TryParse(tbStock.Text, out int stock))
                book.Stock = stock;

            book.Description = tbDescription.Text.Trim();

            if (isNew)
                DBClass.connect.Books.Add(book);

            var log = new Logs
            {
                UserID = App.CurrentUser.UserID,
                ActionType = isNew ? "Добавление" : "Редактирование",
                ActionDescription = isNew ? $"Добавлена книга '{book.Title}'" : $"Отредактирована книга '{book.Title}'",
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
