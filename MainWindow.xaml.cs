using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Oblig3.Models;
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

namespace Oblig3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Dat154Context dx = new();

        private readonly LocalView<Student> Students;

        public MainWindow()
        {
            InitializeComponent();

            Students = dx.Students.Local;

            dx.Students.Load();

            studentList.ItemsSource = Students.OrderBy(s => s.Studentname);
        }
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                studentList.ItemsSource = Students.OrderBy(s => s.Studentname);
            }
            else
            {
                var filteredList = Students.Where(s => s.Studentname.ToLower().Contains(searchText)).ToList();
                studentList.ItemsSource = filteredList;
            }
        }
    }
}
