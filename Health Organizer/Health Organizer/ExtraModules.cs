using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Organizer
{
    class ExtraModules
    {
        public static String RemoveStringSpace(String InputString){
            String result="";
            
            foreach (String temp in InputString.Split(' '))
            {
                if (temp.Equals(""))
                    continue;
                
                 result += temp + " ";
            }
            return result;
        }

        public static double CalculateBMI(int feet, int inch, int weight)
        {
            return weight / (((feet * 12) + inch) * 0.0254); 
        }
    }
}
