using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Health_Organizer
{
    class ExtraModules
    {
        public static String RemoveStringSpace(String InputString)
        {
            String OutputString = "";

            foreach (String temp in InputString.Split(' '))
            {
                if (temp.Equals(""))
                    continue;

                OutputString += temp + " ";
            }
            return RemoveRepeatedPart(OutputString);
        }

        public static double CalculateBMI(int feet, int inch, int weight)
        {
            return weight / (((feet * 12) + inch) * 0.0254);
        }

        public static String RemoveStringNewLine(String InputString)
        {
            String OutputString = Regex.Replace(InputString, @"\t|\n|\r", "");

            return RemoveRepeatedPart(OutputString);
        }

        public static String RemoveRepeatedPart(String InputString)
        {
            String OutputString = "";

            foreach (string str in InputString.Split(','))
            {
                if (OutputString.IndexOf(str) == -1)
                {
                    OutputString = OutputString + "," + str;
                }
            }

            return OutputString;
        }

        public static String RemoveExtraCommas(String input)
        {
            String result = "", temp = "";
            foreach (var i in input.Split(','))
            {
                temp = i;
                if (temp.Trim().Equals(""))
                    continue;
                result += i + ",";
            }
            if (!result.Equals(""))
                result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}
