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
            courseStudentList.ItemsSource = Grades.OrderBy(g => g.Coursecode);

            courseComboBox.DisplayMemberPath = "Coursecode";

        }
        // Class that handles search for studentnames. The list is updated based on the 
        // students names that contain the characters searched for.
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
               // If the search field is empty, all the students are displayed
                    studentList.ItemsSource = Students.OrderBy(s => s.Studentname);
            }
            else
            {
                // The list is filtered to student names that contain the searched for characters
                var filteredList = Students.Where(s => s.Studentname.ToLower().Contains(searchText)).ToList();
                studentList.ItemsSource = filteredList;
            }
        }
        // Class that handles the course search
        private void searchCourseButton_Click(object sender, RoutedEventArgs e)
        {
                // If something is selected in the combobox, the list is filtered to the relevant Coursecode
                // and joins Student, and Course with Grade in order to aquire Coursename and Studentname
                if (courseComboBox.SelectedItem != null)
                {
                    Course selectedCourse = (Course)courseComboBox.SelectedItem;
                    string selectedCourseCode = selectedCourse.Coursecode;
                var filteredList2 = from grade in Grades
                                    join student in Students on grade.Studentid equals student.Id
                                    join course in Courses on grade.Coursecode equals course.Coursecode
                                    where grade.Coursecode == selectedCourseCode
                                    select new
                                    {
                                        Studentname = student.Studentname,
                                        Studentid = student.Id,
                                        Coursecode = grade.Coursecode,
                                        Semester = course.Semester,
                                        Teacher = course.Teacher,
                                        Coursename = course.Coursename,
                                        Grade1 = grade.Grade1
                                    };
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
                var filteredList3 = from grade in Grades
                                    join student in Students on grade.Studentid equals student.Id
                                    join course in Courses on grade.Coursecode equals course.Coursecode
                                    where GetGradeValue(grade.Grade1) >= GetGradeValue(gradeSearchText)
                                    select new
                                    {
                                        Studentname = student.Studentname,
                                        Studentid = student.Id,
                                        Coursecode = grade.Coursecode,
                                        Semester = course.Semester,
                                        Teacher = course.Teacher,
                                        Coursename = course.Coursename,
                                        Grade1 = grade.Grade1
                                    };

                GradeList.ItemsSource = filteredList3;
            }
            else
            {
                // If no input was given after pressing the search button, all grades are displayed.
                var filteredList3 = from grade in Grades
                                    join student in Students on grade.Studentid equals student.Id
                                    join course in Courses on grade.Coursecode equals course.Coursecode
                                    orderby grade.Studentid
                                    select new
                                    {
                                        Studentname = student.Studentname,
                                        Studentid = student.Id,
                                        Coursecode = grade.Coursecode,
                                        Semester = course.Semester,
                                        Teacher = course.Teacher,
                                        Coursename = course.Coursename,
                                        Grade1 = grade.Grade1
                                    };

                GradeList.ItemsSource = filteredList3;
            }
        }
        //Class that handles the search for students that failed classes.
        private void searchFailedButton_Click(object sender, RoutedEventArgs e)
        {
            var filteredList4 = from grade in Grades
                                join student in Students on grade.Studentid equals student.Id
                                join course in Courses on grade.Coursecode equals course.Coursecode
                                where GetGradeValue(grade.Grade1) == GetGradeValue("F") //All Grades that equals F are selected
                                select new
                                {
                                    Studentname = student.Studentname,
                                    Studentid = student.Id,
                                    Coursecode = grade.Coursecode,
                                    Semester = course.Semester,
                                    Teacher = course.Teacher,
                                    Coursename = course.Coursename,
                                    Grade1 = grade.Grade1
                                };

            FailedList.ItemsSource = filteredList4;
        }

        //Class for adding students to a course
        private void addStudentButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Dat154Context())
            {
                var courseId = courseIdTextBox.Text;
                var studentId = studentIdTextBox.Text;
                var grade = gradeTextBox.Text;

                var course = context.Courses.SingleOrDefault(c => c.Coursecode == courseId);
                var student = context.Students.SingleOrDefault(s => s.Id == int.Parse(studentId));

                if (course == null)
                {
                    participantResultTextBlock.Text = "Course not found";
                    return;
                }

                if (student == null)
                {
                    participantResultTextBlock.Text = "Student not found";
                    return;
                }

                var existingGrade = context.Grades.FirstOrDefault(g => g.Studentid == int.Parse(studentId) && g.Coursecode == courseId);
                if (existingGrade != null)
                {
                    participantResultTextBlock.Text = "Student is already enrolled in the course";
                    return;
                }

                var gradeObj = new Grade { Studentid = int.Parse(studentId), Coursecode = courseId, Grade1 = grade };
                context.Grades.Add(gradeObj);
                context.SaveChanges();

                participantResultTextBlock.Text = "Student added to course";

                dx.Grades.Load();
                dx.Students.Load();
                dx.Courses.Load();
            }
        }
        // Class for removing students from a course
        private void removeStudentButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Dat154Context())
            {
                var courseId = courseIdTextBox.Text;
                var studentId = studentIdTextBox.Text;

                var existingGrade = context.Grades.FirstOrDefault(g => g.Studentid == int.Parse(studentId) && g.Coursecode == courseId);
                if (existingGrade == null)
                {
                    participantResultTextBlock.Text = "Student is not enrolled in the course";
                    return;
                }

                context.Grades.Remove(existingGrade);
                context.SaveChanges();

                participantResultTextBlock.Text = "Student removed from course";

                dx.Grades.Load();
                dx.Students.Load();
                dx.Courses.Load();
            }
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

