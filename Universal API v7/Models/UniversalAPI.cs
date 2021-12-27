using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Universal_API_v7.Models
{
    public class UniversalAPI
    {
        public string Program_Code { get; set; }
        public string Key_Code { get; set; }
        public string Module { get; set; }
        public string Object { get; set; }
        public string Function { get; set; }
        public int Version { get; set; }
        public string Procedure { get; set; }
        public string Parameters { get; set; }
        public ArrayList Values { get; set; }
        //public DateTime Date_Created { get; set; }
        public string Location_Created { get; set; }

    }

    public class SP_Name_Parameters
    {
        public string Schema { get; set; }
        public string Procedure { get; set; }
        public string ConnectionString { get; set; }
        public string PARAMETER_NAME { get; set; }

    }
}
