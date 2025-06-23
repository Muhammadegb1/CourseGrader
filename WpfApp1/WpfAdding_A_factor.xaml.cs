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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for WpfAdding_A_factor.xaml
    /// </summary>
    public partial class WpfAdding_A_factor : Window
    {

        #region --Properties--

        private Dictionary<string, int> taskFactors;
        public string SelectedTask { get; private set; }
        public List<string> Tasks { get; set; }
        public int Factor
        {
            get
            {
                if (SelectedTask != null && taskFactors.ContainsKey(SelectedTask))
                    return taskFactors[SelectedTask];
                return 0;
            }
            private set
            {
                if (SelectedTask != null)
                    taskFactors[SelectedTask] = value;

            }
        }
        #endregion


        #region --Constractor--
        public WpfAdding_A_factor(List <string> tasks,Dictionary<string,int> factor):this()
        {
            taskFactors = factor ?? new Dictionary<string, int>(); // Assign if factor is not null, else create a new dictionary.
            Tasks = tasks;
            taskComboBox.ItemsSource = Tasks;

        }
        public WpfAdding_A_factor()
        {
            InitializeComponent();
        }
        #endregion



        #region --Methods--

        private void UpdateFactorButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTask = taskComboBox.SelectedItem as string; 
            if (selectedTask != null && factorTextBox.Text != null) // Checks that I entered a factor when the button is clicked
            {
                int factor = int.Parse(factorTextBox.Text);
                if (factor >= 0 )
                {
                    this.SelectedTask = selectedTask;
                    this.Factor = factor;
                    taskFactors[this.SelectedTask] = factor;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void taskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTask = taskComboBox.SelectedItem as string; // המרה
            if (taskFactors != null && taskFactors.ContainsKey(selectedTask))
            {
                factorTextBox.Text = taskFactors[selectedTask].ToString();
            }
            else
                factorTextBox.Text = "0";
        }
        #endregion

    }
}
