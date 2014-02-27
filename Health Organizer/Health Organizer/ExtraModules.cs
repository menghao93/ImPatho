using EASendMailRT;
using Health_Organizer.Data_Model_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace Health_Organizer
{
    class ExtraModules
    {
        public static string datePatt = @"yyyy-MM-d HH:mm:ss";
        public static string domain_address = "http://pharmassist.co.in/ic2k14";
        public static string RemoveStringSpace(String InputString)
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

        public static string RemoveStringNewLine(String InputString)
        {
            String OutputString = Regex.Replace(InputString, @"\t|\n|\r", "");

            return RemoveRepeatedPart(OutputString);
        }

        public static string RemoveRepeatedPart(String InputString)
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

        public static string RemoveExtraCommas(String input)
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

        public static string getMartialStatus(bool status)
        {
            if (status)
            {
                return "Married";
            }
            else
            {
                return "Unmarried";
            }
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
            catch (Exception ex)
            {
                Debug.WriteLine("Date was not able to be converted: " + year + " " + month + " " + day);
                Debug.WriteLine(ex.ToString());
                c_date = new DateTime(1980, 1, 1);
            }

            return c_date;
        }

        public static bool isEmail(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
        }

        public static string getFileDataForAnalysisItem(AnalysisSampleDataItem selectedItem)
        {
            if (selectedItem == null)
            {
                return "";
            }

            string data = "";
            string columnSeparator = ", ";
            string lineSeparator = "\r\n";

            data += "Name" + columnSeparator;
            data += selectedItem.Name + columnSeparator + lineSeparator;
            data += "Blood Group" + columnSeparator;
            data += selectedItem.BloodGroup + columnSeparator + lineSeparator;
            data += "Sex " + columnSeparator;
            data += selectedItem.Sex + columnSeparator + lineSeparator;
            data += "Martial Status " + columnSeparator;
            data += ExtraModules.getMartialStatus(selectedItem.Married) + columnSeparator + lineSeparator;
            data += "Occupation" + columnSeparator;
            data += selectedItem.Occupation + columnSeparator + lineSeparator;

            data += lineSeparator + "Allergies";

            foreach (string allergy in selectedItem.Allergy)
            {
                data += columnSeparator;
                data += allergy + columnSeparator + lineSeparator;
            }

            data += lineSeparator + "Addiction";
            foreach (string addiction in selectedItem.Addiction)
            {
                data += columnSeparator;
                data += addiction + columnSeparator + lineSeparator;
            }

            data += lineSeparator + "Operation";
            foreach (string operation in selectedItem.Operation)
            {
                data += columnSeparator;
                data += operation + columnSeparator + lineSeparator;
            }

            if (selectedItem.DatesVisited.Count > 0)
            {
                data += lineSeparator + "Visits" + lineSeparator;

                foreach (string date in selectedItem.DatesVisited)
                {
                    data += date + columnSeparator;
                    string disease = "";
                    selectedItem.Diseases.TryGetValue(date, out disease);
                    string vaccine = "";
                    selectedItem.Vaccines.TryGetValue(date, out vaccine);
                    data += disease + columnSeparator + vaccine + lineSeparator;
                }
            }

            return data;
        }

        public static async Task Send_Email(string from, string password, string to, string subj, string body, string filePath)
        {
            String Result = "";
            try
            {
                SmtpMail oMail = new SmtpMail("TryIt");
                SmtpClient oSmtp = new SmtpClient();

                // Set sender email address, please change it to yours
                oMail.From = new MailAddress(from);

                // Add recipient email address, please change it to yours
                oMail.To.Add(new MailAddress(to));

                // Add attachment
                Attachment oAttachment = await oMail.AddAttachmentAsync(filePath);

                // Set email subject and email body text
                oMail.Subject = subj;
                oMail.TextBody = body;

                if (!getSMTPserver(from).Equals(""))
                {
                    // Your SMTP server address
                    SmtpServer oServer = new SmtpServer(getSMTPserver(from));

                    // User and password for SMTP authentication            
                    oServer.User = from;
                    oServer.Password = password;

                    // If your SMTP server requires TLS connection on 25 port, please add this line
                    // oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                    // If your SMTP server requires SSL connection on 465 port, please add this line
                    oServer.Port = 465;
                    oServer.ConnectType = SmtpConnectType.ConnectSSLAuto; // or SmtpConnectType.ConnectDirectSSL;

                    await oSmtp.SendMailAsync(oServer, oMail);
                    Result = "Email was sent successfully!";
                }
                else
                {
                    Result = "Host not found";
                }
            }
            catch (Exception ep)
            {
                Result = String.Format("Failed to send email with the following error: {0}", ep.Message);
                if (ep.Message.Contains("Password"))
                {
                    Result = "Failed to send Email: Incorrect Username or Password";
                }
                if (ep.Message.Contains("such host"))
                {
                    Result = "Failed to send Email: No Internet Connection";
                }
            }

            // Display Result by Diaglog box
            Windows.UI.Popups.MessageDialog dlg = new
                Windows.UI.Popups.MessageDialog(Result);

            await dlg.ShowAsync();
        }

        public static string getSMTPserver(string email)
        {
            string emailType = email.Substring(email.IndexOf("@"));

            if (emailType.Contains("gmail.com"))
            {
                return "smtp.gmail.com";
            }
            if (emailType.Contains("yahoo"))
            {
                return "smtp.mail.yahoo.com";
            }
            if (emailType.Contains("rediff"))
            {
                return "smtp.rediffmail.com";
            }
            if (emailType.Contains("live") || emailType.Contains("hotmail"))
            {
                return "smtp.live.com";
            }

            return "";
        }
        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        public static string removesemicolon(string str)
        {
            if (str.Contains(";"))
            {
                str = str.Replace(";", ". ");
            }
            return str;
        }
        public static String stringFirstCapital(String input)
        {
            String output;
            input = input.ToLower();
            output = Char.ToUpper(input[0]) + input.Substring(1);
            return output;
        }
    }
}
