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

        public MainWindow()
        {
            InitializeComponent();

            Students = dx.Students.Local;
            Courses = dx.Courses.Local;
            Grades = dx.Grades.Local;

            dx.Students.Load();
            dx.Courses.Load();
            dx.Grades.Load();
            
            studentList.ItemsSource = Students.OrderBy(s => s.Studentname);

            courseComboBox.ItemsSource = Courses.OrderBy(c => c.Coursecode);
            courseComboBox.DisplayMemberPath = "Coursecode";
            courseStudentList.ItemsSource = Grades.OrderBy(g => g.Coursecode);

        }
        // Funksjon som håndterer søk av student.  listen oppdateres ved søk på navn til å bare inkludere
        // studenter som har bokstavene som blir søkt på i navnet sitt.
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
               // Viser først alle studenter når det ikke er oppgitt noe i søkefeltet
                    studentList.ItemsSource = Students.OrderBy(s => s.Studentname);
            }
            else
            {
                // listen oppdateres ved søk på navn til å bare inkludere studenter som har bokstavene som blir søkt på i navnet sitt.
                var filteredList = Students.Where(s => s.Studentname.ToLower().Contains(searchText)).ToList();
                studentList.ItemsSource = filteredList;
            }
        }
        // Funksjon som håndterer søk av emne
        private void searchCourseButton_Click(object sender, RoutedEventArgs e)
        {
            // Dersom det blir valgt noe i comboboksen. Filtreres listen basert på emnekode i Grades og viser studentId, emnekode og karakter.
            if (courseComboBox.SelectedItem != null) { 
                Course selectedCourse = (Course)courseComboBox.SelectedItem;
            string selectedCourseCode = selectedCourse.Coursecode;
                var filteredList2 = Grades.Where(g => g.Coursecode == selectedCourseCode).ToList();
                courseStudentList.ItemsSource = filteredList2;
            }
        }
        // Function that handles grade search
        private void searchGradeButton_Click(object sender, RoutedEventArgs e)
        {
            String gradeSearchText = gradeSearchBox.Text.Trim().ToUpper();
            // If input is given it compares the given grade with the other grades in the DB. 
            // The grades are converted to numerical values by the help function GetGradeValue
            // If the other grades have the same or bigger numerical value they are displayed.

            if (!string.IsNullOrEmpty(gradeSearchText))
            {
                    var filteredList3 = Grades.Where(g => GetGradeValue(g.Grade1) >= GetGradeValue(gradeSearchText)).ToList();
                    GradeList.ItemsSource = filteredList3;
            }
            else
            {
                // If no input was given after pressing the search button, all grades are displayed.
                var filteredList3 = Grades.OrderBy(g => g.Studentid);
                GradeList.ItemsSource = filteredList3;
            }
        }
        //Class that handles the search for students that failed classes.
        private void searchFailedButton_Click(object sender, RoutedEventArgs e)
        {
            // Checks all grades and if they are equal to the numerical value for F (in other words they got an F) 
            // If grade equals F it is put inito the filteres list
            var filteredList4 = Grades.Where(g => GetGradeValue(g.Grade1).Equals(GetGradeValue("F"))).ToList();
            FailedList.ItemsSource = filteredList4; 
        }

        //Help class for converting grades to numerical values.
        private int GetGradeValue(string grade)
        {
            return grade switch
            {
                "A" => 5,
                "B" => 4,
                "C" => 3,
                "D" => 2,
                "E" => 1,
                "F" => 0,
                _ => -1 // invalid grade
            };
        }
        // Help class for checking valid input
        private bool IsValidGrade(string grade)
        {
            // Convert the grade to uppercase for case-insensitive validation
            grade = grade.ToUpper();

            // Check if the grade is between F and A
            return grade.Length == 1 && grade[0] >= 'F' && grade[0] <= 'A';
        }
    }
}
