using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Student
    {
        #region --Properties--

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ZehutNumber { get; set; }
        public int Year { get; set; }
        public Dictionary<string, double> Grades { get; set; } // Key = name assignement  value = ציון המטלה
        public Dictionary<string, double> Grades_No_Changes { get; set; }


        public string FinalGrade { get; set; }
        #endregion
        public override string ToString()
        {
            return $"Name: {this.FirstName} \n" +
                   $"Last Name: {LastName} \n" +
                   $"ID: {ZehutNumber} \n" +
                   $"Year:{Year}"; //Student Info Data
        }
    }
}
