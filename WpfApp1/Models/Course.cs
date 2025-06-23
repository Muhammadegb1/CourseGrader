using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Course
    {
        #region --Properties--
        public string NameCourse { get; set; }
        public List<Student> Students { get; set; }
        public double Average { get; set; } = 0 ;

        public Dictionary<string,string> Task {  get; set; } // Key = name assignement , value = אחוז המטלה

        public Dictionary<string,int> Factor { get; set; } = new Dictionary<string, int>();

        #endregion
        public override string ToString()
        {
            string stude = "";
            foreach (var student in Students)
            {
                stude += student.FirstName + " " + student.ZehutNumber + '\n';
            }
            return stude;
        }
    }
}
