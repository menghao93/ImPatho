using Health_Organizer.Data;
using SQLiteWinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Health_Organizer
{
    public sealed partial class Settings : UserControl
    {
        private Database database;

        public Settings()
        {
            this.InitializeComponent();
            database = App.database;
        }

        private async void SettingsSynClicked(object sender, RoutedEventArgs e)
        {
            if (ExtraModules.IsInternet())
            {
                string statements = await sendToServer("1");
                var data = new List<KeyValuePair<string, string>>
                     {
                         new KeyValuePair<string, string>("userid","14"),
                         new KeyValuePair<string, string>("auth_token", "28b60a16b55fd531047c"),
                         new KeyValuePair<string, string>("updatestatements",statements)
                      };
               await  Uploadtoserver(data);
            }
        }

        private async Task<string> Uploadtoserver(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://localhost:63342/Ic2014/UpdateServerdata.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Sync Output" + responseString);
                JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                string error = root.GetNamedString("error");
                return error;
            }
            return "Check internet Connection";
        }

        public async void getFromServer(string BigQuery)
        {
            try
            {
                foreach (string singleQuery in BigQuery.Split(new string[] { ";" }, StringSplitOptions.None))
                {
                    Statement statement = await this.database.PrepareStatementAsync(singleQuery);
                    await statement.StepAsync();
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("MainMenuPage---forFromServer" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        public async Task<string> sendToServer(string TimeStamp)
        {
            string[] tableNames = new string[] { "Patient", "MutableDetails", "MutableDetailsAllergy", 
                "MutableDetailsAddiction", "MutableDetailsOperation", "Address", "AddressZIP", "AddressCity", "AddressState", "MedicalDetails", 
                "MedicalDetailsMedicine", "MedicalDetailsVaccine" };
            string output = "";
            try
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    string columnanmes = await getColumnames(tableNames[i]);
                   Statement statement = await database.PrepareStatementAsync("SELECT "+ columnanmes +" from " + tableNames[i] + " where TimeStamp > " + TimeStamp);
                    statement.EnableColumnsProperty();
                    while (await statement.StepAsync())
                    {
                        string[] seprated_columnnames = columnanmes.Split(new string[] { "," }, StringSplitOptions.None);
                        //Debug.WriteLine("values: " + statement.Columns.SelectMany(x => x.Value));
                        string[] temp = new string[seprated_columnnames.Length];
                        for (int j = 0; j < seprated_columnnames.Length; j++)
                        {
                            temp[j] = "'"+statement.Columns[seprated_columnnames[j]]+"'";
                        }
                        String values = String.Join(", ",temp);
                        output += "REPLACE into " + tableNames[i] + "( " + columnanmes + ",Userid) values (" + values + ",14);";
                    }
                }
                Debug.WriteLine("output: " + output);
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("MainMenuPage---sendtoserver" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return output;
        }

        public async Task<string> getColumnames(string tableName)
        {
            string columnNames = "";
            try
            {
                // read column names\
                Statement statement = await database.PrepareStatementAsync("PRAGMA table_info(" + tableName + ")");
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    columnNames += statement.Columns["name"].ToString() + ",";
                }
                return columnNames.Substring(0, columnNames.Length - 1);
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("MainMenuPage---getColumnname" + "\n" + ex.Message + "\n" + result.ToString());
                return "";
            }
        }

    }
}
