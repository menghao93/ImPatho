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
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Health_Organizer
{
    public sealed partial class SettingsFlyout1 : SettingsFlyout
    {
        Popup _p;
        static Border flyout_border;
        private static Database database;

        public SettingsFlyout1()
        {
            this.InitializeComponent();
            database = App.database;
            BackClick += SettingsFlyout1_BackClick;
            Unloaded += SettingsFlyout1_Unloaded;
            Tapped += SettingsFlyout1_Tapped;
        }

        void SettingsFlyout1_BackClick(object sender, BackClickEventArgs e)
        {
            flyout_border.Child = null;
            SettingsWaitTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

        }

        void SettingsFlyout1_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_p != null)
            {
                _p.IsOpen = false;
            }
            SettingsWaitTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        void SettingsFlyout1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public async void ShowCustom()
        {
            SettingsOrganizationName.Text = await getOrganisationName();
            SettingsUserName.Text = await getUserName();
            _p = new Popup();
            Border b = new Border();
            flyout_border = b;
            b.ChildTransitions = new TransitionCollection();

            // TODO: if you support right-to-left builds, make sure to test all combinations of RTL operating
            // system build (charms on left) and RTL flow direction for XAML app.  EdgeTransitionLocation.Left
            // may need to be used for RTL (and HorizontalAlignment.Left on the SettingsFlyout below).
            b.ChildTransitions.Add(new EdgeUIThemeTransition() { Edge = EdgeTransitionLocation.Right });

            b.Background = new SolidColorBrush(Colors.Transparent);
            b.Width = Window.Current.Bounds.Width;
            b.Height = Window.Current.Bounds.Height;
            b.Tapped += b_Tapped;

            this.HorizontalAlignment = HorizontalAlignment.Right;
            b.Child = this;

            _p.Child = b;
            _p.IsOpen = true;
        }

        void b_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border b = (Border)sender;
            b.Child = null;
        }
        //sync functions 
        private async void SettingsSynClicked(object sender, RoutedEventArgs e)
        {
            SettingsWaitTextBlock.Text = "Syncing";
            SettingsWaitTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SettingsSyncButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            settingsProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (ExtraModules.IsInternet())
            {
                string timestamp = await getTimeStamp();//"1/25/2014 3:34:06 AM";
                string userid = await getUserId();
                string auth_token = await getAuthToken();
                string statements = await sendToServer(timestamp, userid);
                var data = new List<KeyValuePair<string, string>>
                     {
                         new KeyValuePair<string, string>("userid",userid),
                         new KeyValuePair<string, string>("auth_token", auth_token),
                         new KeyValuePair<string, string>("updatestatements",statements)
                      };
                var datatorecieve = new List<KeyValuePair<string, string>>
                     {
                         new KeyValuePair<string, string>("userid",userid),
                         new KeyValuePair<string, string>("auth_token", auth_token),
                         new KeyValuePair<string, string>("timestamp",timestamp)
                      };

                await Uploadtoserver(data);
                await getfromserver(datatorecieve);
                SettingsWaitTextBlock.Text = "Done";
            }
            else
            {
                SettingsWaitTextBlock.Text = "Failed";
            }
            SettingsSyncButton.IsEnabled = true;
            settingsProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SettingsSyncButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            
           // SettingsWaitTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task Uploadtoserver(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(ExtraModules.domain_address + "/UpdateServerdata.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    Debug.WriteLine("Sync Output" + responseString);
                    // JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                    // string error = root.GetNamedString("error");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in Sync");
                    Debug.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Debug.WriteLine("Check internet Connection");
            }
        }

        private async Task<string> getfromserver(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(ExtraModules.domain_address + "/updatetomachine.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                //Debug.WriteLine("Sync Output" + responseString);
                string error = responseString.ToString();
                //JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                //string error = root.GetNamedString("query");
                await aftergetFromServer(error);
                string userid = await getUserId();
                updateTimeStamp(userid);
                return "done";
            }
            return "Check internet Connection";
        }

        public async Task aftergetFromServer(string BigQuery)
        {
            try
            {
                string[] singleQuery = BigQuery.Split(new string[] { ";" }, StringSplitOptions.None);
                for (int i = 0; i < singleQuery.Length; i++)
                //foreach (string singleQuery in BigQuery.Split(new string[] { ";" }, StringSplitOptions.None))
                {
                    Debug.WriteLine(singleQuery[i] + ";");
                    Statement statement = await SettingsFlyout1.database.PrepareStatementAsync(singleQuery[i]);
                    statement.EnableColumnsProperty();
                    await statement.StepAsync();
                }
                Debug.WriteLine("sync complete");
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("sync setting---forFromServer" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        public async Task<string> sendToServer(string TimeStamp, string userid)
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
                    Statement statement = await database.PrepareStatementAsync("SELECT " + columnanmes + " from " + tableNames[i] + " where TimeStamp > '" + TimeStamp + "'");
                    statement.EnableColumnsProperty();
                    while (await statement.StepAsync())
                    {
                        string[] seprated_columnnames = columnanmes.Split(new string[] { "," }, StringSplitOptions.None);
                        string[] temp = new string[seprated_columnnames.Length];
                        for (int j = 0; j < seprated_columnnames.Length; j++)
                        {

                            temp[j] = "'" + statement.Columns[seprated_columnnames[j]] + "'";
                        }
                        String values = String.Join(", ", temp);

                        output += "REPLACE into " + tableNames[i].ToLower() + "( " + columnanmes + ",Userid) Values (" + values.ToString() + "," + userid + ");";
                    }
                }
                Debug.WriteLine("output: " + output.ToString());
                return output;
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


        private async Task<string> getUserId()
        {
            String userid = "";
            try
            {
                Statement statement = await database.PrepareStatementAsync("SELECT UserId FROM UserDetails");
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    userid = statement.Columns["UserId"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("Settings---getuserid" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return userid;
        }

        private async Task<string> getAuthToken()
        {
            String auth_token = "";
            try
            {
                Statement statement = await database.PrepareStatementAsync("SELECT Auth_Token FROM UserDetails");
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    auth_token = statement.Columns["Auth_Token"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("Settings---getAuthtoken" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return auth_token;
        }

        private async Task<string> getTimeStamp()
        {
            String timestamp = "";
            try
            {
                Statement statement = await database.PrepareStatementAsync("SELECT TimeStamp FROM UserDetails");
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    timestamp = statement.Columns["TimeStamp"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return timestamp;
        }

        private async void updateTimeStamp(String Userid)
        {
            try
            {
                String updateQuery = "Update UserDetails Set TimeStamp = @ts Where UserId = @userid";
                Statement statement = await SettingsFlyout1.database.PrepareStatementAsync(updateQuery);
                statement.BindTextParameterWithName("@userid", Userid);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("Settings---UPDATETIMESTAMP" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        private async void SettingsLogoutClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("<<<<<<<<<<<<<<<<<" + await getOrganisationName() + ">>>>>>>>>>>>>>>");
        }
        public static async Task<String> getOrganisationName()
        {
            SettingsFlyout1.database = App.database;
            String organisation = "abc";
            try
            {
                string query = "SELECT * FROM UserDetails WHERE Organisation Not LIKE '' Limit 1;";
                Statement statement = await SettingsFlyout1.database.PrepareStatementAsync(query);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    organisation = statement.Columns["Organisation"];
                }
                return organisation;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("MAIN-PAGE---CHECK--LOGIN" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return organisation;
        }
        public async Task<String> getUserName()
        {
            SettingsFlyout1.database = App.database;
            String username = "abc";
            try
            {
                string query = "SELECT * FROM UserDetails WHERE Organisation Not LIKE '' Limit 1;";
                Statement statement = await SettingsFlyout1.database.PrepareStatementAsync(query);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    username = statement.Columns["UserName"];
                }
                return username;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("MAIN-PAGE---CHECK--LOGIN" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return username;
        }
    }
}
