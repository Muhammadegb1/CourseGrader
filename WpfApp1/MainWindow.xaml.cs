using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using System.Text.Json;
using OfficeOpenXml;
using System.IO.Packaging;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Media;
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region --Properties--
        public List<Course> Courses { get; set; }
        public List<Student> Students { get; set; }
        public Dictionary<string, string> Tasks { get; set; } //saved all the Tasks --> <Key = name Task , value = percentage of the assignment >


        /// <summary>
        /// gradeTextBoxes : 
        /// where we will save all the grades of the students with the weight found in the StackPanel
        /// and also the average of each student and at the end the average of the course
        /// </summary>
        private Dictionary<string, TextBox> gradeTextBoxes;
        #endregion

        #region --Constractor--

        public MainWindow()
        {
            InitializeComponent();
            Courses = new List<Course>();
            startCoursesComboBox();
        }
        #endregion
        
        #region --Methods--
        private void startCoursesComboBox()
        {
            string subFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Courses_JSON"); 
            if (!Directory.Exists(subFolderPath))
            {
                Directory.CreateDirectory(subFolderPath);
            }

            string coursesListFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Courses_JSON", "Courses_List.json");

            Dictionary<string, string> coursesDict = new Dictionary<string, string>();

            if (File.Exists(coursesListFilePath))
            {

                string list = File.ReadAllText(coursesListFilePath);
                coursesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(list);

                // Convert each pair of values to a string and collect into a list
                var coursesWithDates = coursesDict.Select(kv => $"{kv.Key}: {kv.Value}").ToList(); // [] כדי להתעלם מ 

                this.CoursesComboBox.ItemsSource = coursesWithDates;
            }
        }


        /// <summary>
        /// Handling the event I choose from the courses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoursesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoursesComboBox.SelectedItem == null) return; //  בדיקה ל- null

            string selectedCourse = (CoursesComboBox.SelectedItem as string).Split(':')[0].Trim(); 
            SelectedCourseNameTextBlock.Text = selectedCourse;
            this.Title = selectedCourse;

            string subFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Courses_JSON");
            string filePath;

            foreach (var course in Courses)
            {
                if (course.NameCourse == selectedCourse)
                {
                    DisplayAverag_For_CourseAndSave(course);

                    CourseAverageTextBlock.Text = Courses.FirstOrDefault(s => s.NameCourse.Equals(course.NameCourse)).Average.ToString(); // Final Grade Average
                    Display_lst_students_And_Info(selectedCourse);        
                    return;
                }
            }

            string searchPattern = $"{selectedCourse}_*.json";

            // Search for files whose name starts with the course name
            string[] files = Directory.GetFiles(subFolderPath, searchPattern);
            filePath = files.Length > 0 ? files[0] : null; 

            string json = File.ReadAllText(filePath);
            Course coursesDict = JsonSerializer.Deserialize<Course>(json);
            Course course_from_file =  Courses.FirstOrDefault(s => s.NameCourse.Equals(coursesDict.NameCourse));
            if (course_from_file != null)
            {
                //course.NameCourse = coursesDict.NameCourse;
                course_from_file.Average = coursesDict.Average;
                course_from_file.Students = coursesDict.Students;
                course_from_file.Task = coursesDict.Task;
                course_from_file.Factor = coursesDict.Factor;
            }
            else // Course not found
            {
                Courses.Add(coursesDict);
            }
                 DisplayAverag_For_CourseAndSave(coursesDict);
                 CourseAverageTextBlock.Text = coursesDict.Average.ToString(); // Final Grade Average For Course
                 Display_lst_students_And_Info(selectedCourse);          
        }

        private void DisplayAverag_For_CourseAndSave(Course course) // Update the course average 
        {
            double sum = 0;
            foreach (var student_grade in course.Students)
                sum += double.Parse(student_grade.FinalGrade);
            double average_for_course = sum / course.Students.Count;
            Courses.FirstOrDefault(s => s.NameCourse.Equals(course.NameCourse)).Average = double.Parse(average_for_course.ToString("0.##")); 
            CourseAverageTextBlock.Text = Courses.FirstOrDefault(s => s.NameCourse.Equals(course.NameCourse)).Average.ToString();
        }


        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "[CSV files] (*.CSV)|*.CSV";
            string fileName = "";
            string path = "";
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    GradesStackPanel.Children.Clear();
                    this.lstStudents.ItemsSource = "";
                    Student_Info_Data.Text = "";
                    Course course = new Course();

                    fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // name file -> is name course
                    path = openFileDialog.FileName; // full path
                    TextLoad.Text = path;
                    this.Title = fileName;
                    course.NameCourse = fileName;


                    LoadStudentsFromCSVFile(path);
                    if (Students == null) // if the cols of assignments > 10 
                        return;
                    course.Students = Students.OrderBy(s => s.FirstName).ToList();
                    course.Task = Tasks;
                    var existingCourse = Courses.FirstOrDefault(s => s.NameCourse.Equals(course.NameCourse));
                    if (existingCourse != null)
                    {
                        Courses.Remove(existingCourse);
                    }
                    Courses.Add(course);

                    save_CourseIn_Data(course);
                    startCoursesComboBox();
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Failed to open file: " + error.Message);
                return;
            }
        }

        private void Display_lst_students_And_Info(string name) //Displaying the data of each student
        {
            foreach (var course in Courses) 
            {
                if (course.NameCourse == name)
                {
                    string courseStudentsString = course.ToString();
                    // Decomposing a string into an array of strings, each representing a single student
                    var studentsArray = courseStudentsString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                   // Setting the string array as the ListBox's ItemsSource
                    this.lstStudents.ItemsSource = studentsArray;
                    lstStudents.SelectedIndex = 0;
                    lstStudents.Focus();
                    return;
                }
            }
        }



        private void LoadStudentsFromCSVFile(string filePath) //Reading a file Course for the first time 
        {
            Students = new List<Student>();

            var lines = File.ReadAllLines(filePath);

            var cols = lines[0].Split(',');
            if (cols.Length - 4 > 10)
            {
                MessageBox.Show("The count of cols for assignments is greater than 10");
                return;
            }

            Tasks = new Dictionary<string, string>();
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                Student student = new Student
                {
                    FirstName = values[0],
                    LastName = values[1],
                    ZehutNumber = Convert.ToInt32(values[2]),
                    Year = Convert.ToInt32(values[3]),
                    Grades = new Dictionary<string, double>(),
                    Grades_No_Changes = new Dictionary<string, double>(),
                };
                double sum = 0;

                //  assumed that there are 4 columns before the grade columns of the assignments
                int enter = 0;
                for (int col = 4; col < values.Length; col++)
                {

                    //   that the course names and percentages are in the column headings in the first row
                    var headerParts = lines[0].Split(',')[col].Split(new string[] { " - " }, StringSplitOptions.None);
                    string assignment = headerParts[0]; // for example, "OOP"
                    string divideGrade = headerParts[1]; // for example, "10%"
                    Tasks[assignment] = divideGrade;  //saved all the assignments with the percentages of each one

                    double divide = double.Parse(divideGrade.Replace("%", "")) / 100;
                    

                    if (double.TryParse(values[col], out double grade) && (grade >= 0 && grade <=100 ))
                    {
                        student.Grades[assignment] = grade;
                        student.Grades_No_Changes[assignment] = grade;
                        sum += divide * grade;
                    }
                    else
                    {
                        student.Grades[assignment] = 0; // If the cell is empty or incorrect
                        student.Grades_No_Changes[assignment] = 0;

                    }
                }
                student.FinalGrade = sum.ToString("0.##");
                Students.Add(student);
            }
            

        }

        // The final grade is updated for student
        public void Final_Grade_Calculator_For_Student(string nameCourse,int zehutNumber)
        {
            
            Course course = Courses.FirstOrDefault(s => s.NameCourse.Equals(nameCourse));
            Student student = course.Students.FirstOrDefault(s => s.ZehutNumber.Equals(zehutNumber));


            /// ==>> Grade< assignment name , grade >.   Task < assignment name,  מטלה אחוז   >
            /// 

            double sum = 0;
            string divideGrade;
            double divide;
            foreach (var grade in student.Grades)
            {
                divideGrade = course.Task[grade.Key];
                divide = double.Parse(divideGrade.Replace("%", "")) / 100 ;   
                sum += grade.Value * divide;
            }
            student.FinalGrade = sum.ToString("0.##");

            gradeTextBoxes[nameCourse].Text = $"Final Grade: {student.FinalGrade}";

        }




        //Saving a course in a JSON file by course name
        private void save_CourseIn_Data(Course course)  ////Save data in file
        {
            string subFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Courses_JSON");  
            string fileName;
            string filePath;

            // Search for files whose name starts with the course name
            string searchPattern = $"{course.NameCourse}_*.json";
            string[] files = Directory.GetFiles(subFolderPath, searchPattern);
            filePath = files.Length > 0 ? files[0] : null; // Take the first file I find, if there is one
            if (filePath != null)
            {
                File.Delete(filePath); 
            }

            fileName = $"{course.NameCourse}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.json"; 
            
            filePath = Path.Combine(subFolderPath, fileName); // The full path to save the file
            string jsonString = JsonSerializer.Serialize(course, new JsonSerializerOptions { WriteIndented = true }); // Converting the data to JSON format
            File.WriteAllText(filePath, jsonString);

            UpdateOrAddCourseInListIn_JSON(course);
        }

        private void UpdateOrAddCourseInListIn_JSON(Course course)
        {
            //  Added the filename to the path
            string coursesListFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Courses_JSON", "Courses_List.json");

            Dictionary<string, string> coursesDict = new Dictionary<string, string>();

            // Reading the contents of the file, if any
            if (File.Exists(coursesListFilePath))
            {
                string json = File.ReadAllText(coursesListFilePath);
                coursesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }

            // Update or add the course
            coursesDict[course.NameCourse] = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");

            string updatedJson = JsonSerializer.Serialize(coursesDict, new JsonSerializerOptions { WriteIndented = true });

            //Saving "updatedJson" to a JSON file
            File.WriteAllText(coursesListFilePath, updatedJson); 

        }







        // Declare a dictionary to hold references to the grade text boxes
        private void StudentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GradesStackPanel.Children.Clear(); // Clear the StackPanel
            Course course = Courses.FirstOrDefault(s => s.NameCourse.Equals(SelectedCourseNameTextBlock.Text));

            gradeTextBoxes = new Dictionary<string, TextBox>();
            if (lstStudents.SelectedIndex >= 0 && lstStudents.SelectedIndex < course.Students.Count)
            {
                int index = lstStudents.SelectedIndex;
                if (course != null)
                {
                    // Displaying the data in a TextBox
                    Student_Info_Data.Text = course.Students[index].ToString();

                    foreach (var grade in course.Students[index].Grades)
                    {
                        StackPanel taskStackPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 5, 0, 5)
                        };

                        TextBlock taskNameTextBlock = new TextBlock // Task
                        {
                            Text = $"{grade.Key}:",
                            Foreground = Brushes.White,
                            Width = 80
                        };

                        TextBox textGrade = new TextBox //Grade for Task
                        {
                            Text = grade.Value.ToString(),
                            Width = 50,
                            Margin = new Thickness(5, 0, 5, 0),
                            HorizontalAlignment = HorizontalAlignment.Left
                        };

                        // Add the TextBox to the dictionary using the grade's key
                        gradeTextBoxes.Add(grade.Key, textGrade);

                        TextBlock percentageTextBlock = new TextBlock  // percentage for Task 
                        {
                            Text = $"{course.Task[grade.Key]}",
                            Foreground = Brushes.White,
                            Width = 50,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };

                        taskStackPanel.Children.Add(taskNameTextBlock);
                        taskStackPanel.Children.Add(textGrade);
                        taskStackPanel.Children.Add(percentageTextBlock);

                        GradesStackPanel.Children.Add(taskStackPanel);
                    }
                    StackPanel s2 = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    TextBox averageTextBlock = new TextBox // final grade for student is presented in textBox
                    {
                        Text = $"Final Grade: {course.Students[index].FinalGrade}",
                        Foreground = Brushes.White,
                        Background = Brushes.CornflowerBlue,
                        Margin = new Thickness(5, 0, 5, 0),
                        IsReadOnly = true,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    gradeTextBoxes.Add(course.NameCourse, averageTextBlock);

                    Button saveButton = new Button
                    {
                        Content = "Save",
                        Margin = new Thickness(20, 0, 5, 0),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };


                    saveButton.Click += new RoutedEventHandler(SaveButton_Click); //save the grades for for student
                    s2.Children.Add(averageTextBlock);
                    s2.Children.Add(saveButton);
                    GradesStackPanel.Children.Add(s2);
                }
                save_CourseIn_Data(course);
            }

        
        }

        /// <summary>
        /// Adding a factor to a certain task that selects it so that we go to another page and there the value of the factor is returned
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FactorButton_Click(object sender, RoutedEventArgs e)
        {
            Course course = Courses.FirstOrDefault(s => s.NameCourse == SelectedCourseNameTextBlock.Text);
            if (course == null) 
            {
                MessageBox.Show("Choose a course from the list of courses or upload a file Course");
                return;
            }
            WpfAdding_A_factor window2_factor = new WpfAdding_A_factor(course.Task.Keys.ToList(), course.Factor); 

            if (window2_factor.ShowDialog() == true)
            {
                //There is access to both the name of the selected assignment and the factor
                course.Factor[window2_factor.SelectedTask] = window2_factor.Factor;
                double sum = 0;
                foreach (var student in course.Students)
                {
                    sum = student.Grades_No_Changes[window2_factor.SelectedTask] + window2_factor.Factor; // Calculates the factor while keeping the initial grade
                    if (sum > 100)
                        sum = 100;
                    student.Grades[window2_factor.SelectedTask] = sum; // We will update the grade after the factor
                    gradeTextBoxes[window2_factor.SelectedTask].Text = sum.ToString();

                    Final_Grade_Calculator_For_Student(course.NameCourse, student.ZehutNumber); // The final grade is updated for each student

                }
                DisplayAverag_For_CourseAndSave(course); 
                save_CourseIn_Data(course);
            }
        }

       // Saving updated grades
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int index = lstStudents.SelectedIndex;

            //Returns the appropriate course with the students in it
            Course selectedCourse = Courses.FirstOrDefault(c => c.NameCourse == SelectedCourseNameTextBlock.Text); 
            Student student = selectedCourse.Students[index];

            foreach (var gradeKey in gradeTextBoxes.Keys)
            {
                if (gradeKey != selectedCourse.NameCourse)
                {
                    // Try to parse the grade from the TextBox, if successful, update the grade
                    if (int.TryParse(gradeTextBoxes[gradeKey].Text, out int updatedGrade) && (updatedGrade <= 100 && updatedGrade >= 0))
                    {
                        student.Grades[gradeKey] = updatedGrade;
                        student.Grades_No_Changes[gradeKey] = updatedGrade;

                    }
                    else // If the grade is not a number or over 100
                    {
                        MessageBox.Show("The grade is invalid");
                        return;
                    }
                }
            }           
            Final_Grade_Calculator_For_Student(selectedCourse.NameCourse, student.ZehutNumber); // The final grade is updated for each student
            DisplayAverag_For_CourseAndSave(selectedCourse); // Update the course average 
            save_CourseIn_Data(selectedCourse);
        }

        #endregion
    }
}