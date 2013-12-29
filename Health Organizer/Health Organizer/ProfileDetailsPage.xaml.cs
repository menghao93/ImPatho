using Health_Organizer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SQLiteWinRT;
using Health_Organizer.Database_Connet_Classes;
using System.Diagnostics;

namespace Health_Organizer
{
    public sealed partial class ProfileDetailsPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private int PID;
        private Database database;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public ProfileDetailsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.PID = Int32.Parse(e.Parameter as string);
            this.InitializeDB();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void InitializeDB()
        {
            this.database = App.database;

            this.LoadDetails();
        }

        private async void LoadDetails()
        {
            try
            {
                string query = "SELECT * FROM Patient WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindInt64ParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();

                if (await statement.StepAsync())
                {
                    ProfileName.Text = statement.Columns["LastName"] + " " + statement.Columns["FirstName"];
                    ProfileImage.Source = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                    ProfileDateOfBirth.Text = statement.Columns["Birthday"];
                    ProfileAge.Text = (DateTime.Now.Year - Convert.ToInt32(statement.Columns["Birthday"].Substring(0, statement.Columns["Birthday"].IndexOf("-")))).ToString(); 
                    ProfileBloodGroup.Text = statement.Columns["BloodGroup"];
                    ProfileSex.Text = statement.Columns["Sex"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---PATIENT" + "\n" + ex.Message + "\n" + result.ToString());
            }

            string street = "", city = "", state = "", country = "", zip = "";
            try
            {
                string query = "SELECT * FROM Address WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    street = statement.Columns["Street"];
                    zip = statement.Columns["ZIP"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---ADDRESS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@zip", Int32.Parse(zip));
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    city = statement.Columns["City"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---ADDRESS_ZIP" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressCity WHERE City = @city";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@city", city);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    state = statement.Columns["State"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---ADDRESS_CITY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressState WHERE State = @state";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@state", state);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    country = statement.Columns["Country"];
                }
                ProfileAddress.Text = street + "\n" + city + ", " + state + ", " + zip + "\n" + country;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---ADDRESS_STATE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string queryDetails = "SELECT * FROM MutableDetails WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(queryDetails);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();

                if (await statement.StepAsync())
                {
                    ProfileContact.Text = statement.Columns["Mobile"];

                    if (statement.Columns["EmMobile"].ToString() != "0")
                    {
                        ProfileEmContact.Text = statement.Columns["EmMobile"];
                    }
                    else
                    {
                        ProfileEmContact.Text = "NA";
                    }

                    ProfileEmail.Text = statement.Columns["Email"];
                    ProfileOccupation.Text = statement.Columns["Occupation"];
                    ProfileFamilyHistory.Text = statement.Columns["FamilyBackground"];

                    if (statement.Columns["Married"].Equals("T"))
                    {
                        ProfileMaritalStatus.Text = "Married";
                    }
                    else
                    {
                        ProfileMaritalStatus.Text = "Unmarried";
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---MUTABLE_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                ProfileAllergies.Text = "";
                string allergyDetails = "SELECT * FROM MutableDetailsAllergy WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(allergyDetails);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    ProfileAllergies.Text += statement.Columns["Allergy"] + ",";
                }
                if (!ProfileAllergies.Text.Equals(""))
                {
                    ProfileAllergies.Text = ProfileAllergies.Text.Substring(0, ProfileAllergies.Text.Length - 1).TrimStart().TrimEnd();
                }
                else
                {
                    ProfileAllergies.Text = "NA";
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---MUTABLE_DETAILS_ALLERGY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                ProfileOperations.Text = "";
                string operationDetails = "SELECT * FROM MutableDetailsOperation WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(operationDetails);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();

                while (await statement.StepAsync())
                {
                    ProfileOperations.Text += statement.Columns["Operation"] + ",";
                }
                if (!ProfileOperations.Text.Equals(""))
                {
                    ProfileOperations.Text = ProfileOperations.Text.Substring(0, ProfileOperations.Text.Length - 1).TrimStart().TrimEnd();
                }
                else
                {
                    ProfileOperations.Text = "NA";
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---MUTABLE_DETAILS_OPERATION" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                ProfileAddictions.Text = "";
                string addictionDetails = "SELECT * FROM MutableDetailsAddiction WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(addictionDetails);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();

                while (await statement.StepAsync())
                {
                    ProfileAddictions.Text += statement.Columns["Addiction"] + ",";
                }
                if (!ProfileAddictions.Text.Equals(""))
                {
                    ProfileAddictions.Text = ProfileAddictions.Text.Substring(0, ProfileAddictions.Text.Length - 1).TrimEnd().TrimStart();
                }
                else
                {
                    ProfileAddictions.Text = "NA";
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("PROFILE_DETAILS_PAGE---LOAD_DETAILS---MUTABLE_DETAILS_ADDICTION" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        private void profileDetailsEditBut(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateProfileForm), this.PID.ToString());
            }
        }
    }
}