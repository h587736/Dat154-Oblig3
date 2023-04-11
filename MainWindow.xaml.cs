using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Oblig3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly LocalView<Course> Courses;
        private readonly LocalView<Grade> Grades;
        private readonly ObservableCollection<Student> CourseStudents = new();
        private string SelectedCourseId;

        public MainWindow()
        {
            InitializeComponent();

            Students = dx.Students.Local;
            Courses = dx.Courses.Local;

            dx.Students.Load();
            dx.Courses.Load();
            dx.Grades.Load();

            studentList.ItemsSource = Students.OrderBy(s => s.Studentname);
            courseComboBox.ItemsSource = Courses.OrderBy(c => c.Coursecode);
            courseComboBox.DisplayMemberPath = "Coursename";
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
        private void searchCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedCourseId))
            {
                var filteredList = dx.Grades.Where(g => g.Coursecode == SelectedCourseId).Select(g => g.Student).ToList();
                courseStudentList.ItemsSource = filteredList.OrderBy(s => s.Id);
            }

        }

        private void courseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (courseComboBox.SelectedItem is Course selectedCourse)
            {
                SelectedCourseId = selectedCourse.Coursecode;
                CourseStudents.Clear();
                var filteredList = dx.Grades
                    .Where(g => g.Coursecode == SelectedCourseId)
                    .Select(g => g.Student)
                    .ToList();
                foreach (var student in filteredList.OrderBy(s => s.Studentname))
                {
                    CourseStudents.Add(student);
                }
                courseStudentList.ItemsSource = CourseStudents;
            }
        }
    }
}
