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
using ForbiddenBookshop.Windows;

namespace ForbiddenBookshop.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        private List<Books> allBooks;
        private List<string> languages;

        public CatalogPage()
        {
            InitializeComponent();
            LoadBooks();
            LoadLanguages();
        }

        private void LoadBooks()
        {
            allBooks = DBClass.connect.Books.ToList();
            icBooks.ItemsSource = allBooks;
        }

        private void LoadLanguages()
        {
            languages = DBClass.connect.Books.Select(b => b.Language).Distinct().ToList();
            foreach (var lang in languages)
            {
                if (!string.IsNullOrEmpty(lang))
                    cmbLanguage.Items.Add(new ComboBoxItem { Content = lang });
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in icBooks.Items)
            {
                var container = icBooks.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container == null) continue;

                var btnOrder = FindChild<Button>(container, "btnOrder");
                var btnDetails = FindChild<Button>(container, "btnDetails");
                var btnEdit = FindChild<Button>(container, "btnEdit");
                var btnDelete = FindChild<Button>(container, "btnDelete");
                var btnManage = FindChild<Button>(container, "btnManage");

                int roleId = App.CurrentUser?.RoleID ?? 2;

                if (btnOrder != null) btnOrder.Visibility = (roleId == 2) ? Visibility.Visible : Visibility.Collapsed;
                if (btnDetails != null) btnDetails.Visibility = Visibility.Visible;
                if (btnEdit != null) btnEdit.Visibility = (roleId == 1) ? Visibility.Visible : Visibility.Collapsed;
                if (btnDelete != null) btnDelete.Visibility = (roleId == 1) ? Visibility.Visible : Visibility.Collapsed;
                if (btnManage != null) btnManage.Visibility = (roleId == 3) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private T FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t && t.Name == name) return t;
                var result = FindChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = tbSearch.Text?.ToLower();
            if (string.IsNullOrEmpty(filter))
                icBooks.ItemsSource = allBooks;
            else
                icBooks.ItemsSource = allBooks.Where(b => b.Title.ToLower().Contains(filter) || (b.Author != null && b.Author.ToLower().Contains(filter))).ToList();
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allBooks == null || cmbLanguage.SelectedItem == null) return;

            var selected = cmbLanguage.SelectedItem as ComboBoxItem;
            if (selected != null)
            {
                if (selected.Content.ToString() == "Все языки")
                    icBooks.ItemsSource = allBooks;
                else
                    icBooks.ItemsSource = allBooks.Where(b => b.Language == selected.Content.ToString()).ToList();
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new BookDetailsPage(id));
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID != 2)
            {
                MessageBox.Show("Только зарегистрированные читатели могут заказывать книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int bookId = (int)((Button)sender).Tag;
            new CreateOrderWindow(bookId).ShowDialog();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID != 1) return;
            int id = (int)((Button)sender).Tag;
            var book = DBClass.connect.Books.Find(id);
            new EditBookWindow(book).ShowDialog();
            LoadBooks();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID != 1) return;
            int id = (int)((Button)sender).Tag;
            if (MessageBox.Show("Удалить книгу? Все связанные заказы и записи о пропавших читателях будут удалены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var book = DBClass.connect.Books.Find(id);
                if (book == null) return;

                try
                {
                    var disappeared = DBClass.connect.DisappearedReaders.Where(d => d.BookID == id).ToList();
                    foreach (var d in disappeared)
                    {
                        DBClass.connect.DisappearedReaders.Remove(d);
                    }

                    var orderBooks = DBClass.connect.OrderBooks.Where(o => o.BookID == id).ToList();
                    foreach (var ob in orderBooks)
                    {
                        DBClass.connect.OrderBooks.Remove(ob);
                    }

                    DBClass.connect.Books.Remove(book);

                    DBClass.connect.SaveChanges();

                    LoadBooks();
                    MessageBox.Show("Книга удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnManage_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID != 3) return;
            int bookId = (int)((Button)sender).Tag;
            var book = DBClass.connect.Books.Find(bookId);
            var disappearedCount = DBClass.connect.DisappearedReaders.Count(d => d.BookID == bookId);
            MessageBox.Show($"Статистика по книге '{book.Title}':\n" +
                            $"Уровень проклятия: {book.CurseLevel}/10\n" +
                            $"Количество пропавших читателей: {disappearedCount}\n" +
                            $"Язык оригинала: {book.Language}\n" +
                            $"Остаток в магазине: {book.Stock} шт.",
                            "Информация о проклятии", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
