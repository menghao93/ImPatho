using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Health_Organizer.Data_Model_Classes
{
    [Table("BasicDiseases")]
    class BasicDiseases
    {
        [MaxLength(45), PrimaryKey]
        public string Name { get; set; }
        
        //We will store Image as Base64 String so the return type would be a String
        public string Image { get; set; }
       
        public string Description { get; set; }
        
        public string Symptoms { get; set;}
    }
}
