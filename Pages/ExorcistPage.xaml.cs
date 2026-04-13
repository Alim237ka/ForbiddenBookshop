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
    /// Логика взаимодействия для ExorcistPage.xaml
    /// </summary>
    public partial class ExorcistPage : Page
    {
        public ExorcistPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.RoleID != 3)
            {
                MessageBox.Show("Нет прав доступа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }
            RefreshData();
        }

        private void RefreshData()
        {
            var highCurseBooks = DBClass.connect.Books.Where(b => b.CurseLevel >= 8).ToList();
            lvHighCurseBooks.ItemsSource = highCurseBooks;

            lvDisappeared.ItemsSource = DBClass.connect.DisappearedReaders.ToList();

            var dangerousBooks = DBClass.connect.Books.OrderByDescending(b => b.CurseLevel).Take(10).ToList();
            lvDangerousBooks.ItemsSource = dangerousBooks;

            var madCustomers = DBClass.connect.Customers.Where(c => c.MadnessLevel >= 70).ToList();
            lvMadCustomers.ItemsSource = madCustomers;
        }

        private void BtnRemoveCurse_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var book = DBClass.connect.Books.Find(id);

            if (MessageBox.Show($"Вы уверены, что хотите снять проклятие с книги '{book.Title}'? Это снизит уровень проклятия до 1.",
                "Снятие проклятия", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var log = new Logs
                    {
                        UserID = App.CurrentUser.UserID,
                        ActionType = "Снятие проклятия",
                        ActionDescription = $"Библиотекарь-экзорцист {App.CurrentUser.FullName} снял проклятие с книги '{book.Title}' (было {book.CurseLevel} -> 1)",
                        ActionDate = DateTime.Now
                    };
                    DBClass.connect.Logs.Add(log);

                    book.CurseLevel = 1;
                    DBClass.connect.SaveChanges();

                    RefreshData();
                    MessageBox.Show("Проклятие успешно снято!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при снятии проклятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
