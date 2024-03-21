using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADI.ADIN.Test
{
    [TestClass]
    public class TrialTest
    {
        public TrialTest()
        {
            School school = new School()
            {
                Classes = new List<SchoolClass>()
        {
            new SchoolClass(){ClassID = 1, ClassName="A", Students = new List<Student>()
                {
                    new Student(){StuID = 1, Name="Maciej", ColorOfSocks="yellow"},
                    new Student(){StuID = 5, Name="Anna", ColorOfSocks="red"},
                    new Student(){StuID = 22, Name="Robert", ColorOfSocks="green"}
                }},
            new SchoolClass(){ClassID = 2, ClassName="B", Students = new List<Student>()
                {
                    new Student(){StuID = 2, Name="Fred", ColorOfSocks="black"},
                    new Student(){StuID = 3, Name="Viana", ColorOfSocks="gray"},
                    new Student(){StuID = 21, Name="Julia", ColorOfSocks="blue"}
                }}
        }
            };

            var cla = school.Classes.Where(c => c.Students.Any(s => s.ColorOfSocks == "red")).ToList();
            //returns the list of all existing classes, because in each class there's at least one studnet with red socks!

            var stu = school.Classes.Select(c => c.Students.Where(s => s.ColorOfSocks == "red").ToList()).ToList();
            //returns the list of students which meets the criteria: {color of socks = red}

            var h = school.Classes.Select(c => c.Students.Any(v => v.ColorOfSocks == "red")).ToList();

        }

        [TestMethod]
        public void TrialTestMethod()
        {

        }
    }

    // Define other methods and classes here

    public class School
    {
        public List<SchoolClass> Classes = new List<SchoolClass>();
    }

    public class SchoolClass
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public List<Student> Students = new List<Student>();
    }

    public class Student
    {
        public int StuID { get; set; }
        public string Name { get; set; }
        public string ColorOfSocks { get; set; }

    }
}
