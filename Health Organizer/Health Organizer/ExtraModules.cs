using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            String trimedString = "";

            //Append all non repeating portion of string in outputString
            foreach (string str in InputString.Split(','))
            {
                if (trimedString.Trim().IndexOf(str.Trim()) == -1)
                {
                    trimedString = trimedString + "," + str;
                    OutputString = OutputString + "," + str;
                }
            }

            //Remove all space from begining and end 
            return OutputString.TrimStart().TrimEnd();
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

        public static DateTime ConvertStringToDateTime(string date)
        {
            string[] tempDate = date.Split('-');
            DateTime c_date;
            int day, month = 1, year;

            year = Convert.ToInt32(tempDate[0]);

            switch (tempDate[1])
            {
                case "January": month = 1;
                    break;

                case "February": month = 2;
                    break;

                case "March": month = 3;
                    break;

                case "April": month = 4;
                    break;

                case "May": month = 5;
                    break;

                case "June": month = 6;
                    break;

                case "July": month = 7;
                    break;

                case "August": month = 8;
                    break;

                case "September": month = 9;
                    break;

                case "October": month = 10;
                    break;

                case "November": month = 11;
                    break;

                case "December": month = 12;
                    break;
            }

            day = Convert.ToInt32(tempDate[2]);

            try
            {
                c_date = new DateTime(year, month, day);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Date was not able to be converted: " + year + " " + month + " " + day );
                Debug.WriteLine(ex.ToString());
                c_date = new DateTime(1980, 1, 1);
            }

            return c_date;
        }
    }
}
