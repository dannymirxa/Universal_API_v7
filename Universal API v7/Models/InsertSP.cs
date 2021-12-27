using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Universal_API_v7.Models
{
    public class InsertSP
    {
        public string Module { get; set; }
        public string Object { get; set; }
        public string Function { get; set; }
        public string Schema { get; set; }
        public string ConnectionString { get; set; }
    }

    public class InsertSPReturn 
    {
        public string Procedure { get; set; }
    }

}
