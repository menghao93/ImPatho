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

namespace Health_Organizer
{
    public sealed partial class ProfileDetailsPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private int PID;
        private DBConnect connection;
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

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.PID = Int32.Parse(e.Parameter as string);
            this.InitializeDB();
        }

        private async void InitializeDB()
        {
            this.connection = new DBConnect();
            await this.connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
            database = this.connection.GetConnection();

            this.LoadDetails();
        }

        private async void LoadDetails()
        {
            string query = "SELECT * FROM Patient WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindInt64ParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();

            if (await statement.StepAsync())
            {
                ProfileName.Text = statement.Columns["LastName"] + " " + statement.Columns["FirstName"];
                ProfileImage.Source = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                ProfileBloodGroup.Text = statement.Columns["BloodGroup"];
                ProfileSex.Text = statement.Columns["Sex"];
                //IMage
                //Age
            }
            // for the address
            String street = "", city = "", state = "", country = "", zip = "";
            query = "SELECT * FROM Address WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                street = statement.Columns["Street"];
                //ZIP code
                zip = statement.Columns["ZIP"];
            }

            query = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@zip", Int32.Parse(zip));
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                city= statement.Columns["City"];
            }
            statement.Reset();
            statement.Reset();
            query = "SELECT * FROM AddressCity WHERE City = @city";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindTextParameterWithName("@city",city);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                state= statement.Columns["State"];
            }
            statement.Reset();

            query = "SELECT * FROM AddressState WHERE State = @state";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindTextParameterWithName("@state", state);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                country = statement.Columns["Country"];
            }
            ProfileAddress.Text = street + "\n" + city + ", " + state + ", "+ zip + "\n" + country;



            string queryDetails = "SELECT * FROM MutableDetails WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(queryDetails);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();

            if (await statement.StepAsync())
            {
                ProfileContact.Text = statement.Columns["Mobile"];
                ProfileEmContact.Text = statement.Columns["EmMobile"];
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


            ProfileAllergies.Text = "";
            string allergyDetails = "SELECT * FROM MutableDetailsAllergy WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(allergyDetails);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                ProfileAllergies.Text += statement.Columns["Allergy"] + ",";
            }
            if (!ProfileAllergies.Text.Equals(""))
            {
                ProfileAllergies.Text = ProfileAllergies.Text.Substring(0, ProfileAllergies.Text.Length - 1);
            }
            statement.Reset();

            ProfileOperations.Text = "";
            string operationDetails = "SELECT * FROM MutableDetailsOperation WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(operationDetails);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();

            while (await statement.StepAsync())
            {
                ProfileOperations.Text += statement.Columns["Operation"] + ",";
            }
            if (!ProfileOperations.Text.Equals(""))
            {
                ProfileOperations.Text = ProfileOperations.Text.Substring(0, ProfileOperations.Text.Length - 1);
            }
            statement.Reset();

            ProfileAddictions.Text = "";
            string addictionDetails = "SELECT * FROM MutableDetailsAddiction WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(addictionDetails);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.EnableColumnsProperty();

            while (await statement.StepAsync())
            {
                ProfileAddictions.Text += statement.Columns["Addiction"] + ",";
            }
            if (!ProfileAddictions.Text.Equals(""))
            {
                ProfileAddictions.Text = ProfileAddictions.Text.Substring(0, ProfileAddictions.Text.Length - 1);
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private void profileDetailsEditBut(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateProfileForm), this.PID.ToString());
            }
        }
    }
}
