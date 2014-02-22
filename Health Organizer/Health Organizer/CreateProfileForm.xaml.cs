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
using System.Diagnostics;
using Health_Organizer.Database_Connet_Classes;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.Media.Capture;
using System.Text;
using Health_Organizer.Data_Model_Classes;
using System.Collections.ObjectModel;

namespace Health_Organizer
{
    public sealed partial class CreateProfileForm : Page
    {
        private string decodedImage = null;
        private Database database;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool isUpdating = false;
        private string updatePID = "-1";
        private string pastGridGroup = null;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public CreateProfileForm()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        private void InitializeCombos()
        {
            //Adding days, months, and years to combobox in form
            for (int i = 0; i < 31; i++)
            {
                profileDayComboBox.Items.Add(i + 1);
            }

            for (int i = 1900; i <= DateTime.Now.Year; i++)
            {
                profileYearComboBox.Items.Add(i);
            }
        }

        private void InitializeDB()
        {
            this.database = App.database;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            this.InitializeDB();
            this.InitializeCombos();
            if (!(e.Parameter as string).Equals("-1"))
            {
                this.isUpdating = true;
                //this.updatePID = Int32.Parse(e.Parameter as string);
                this.updatePID = (e.Parameter as string);
            }

            //If data us edited then we donot need to set the default Image.
            if (isUpdating)
            {
                this.LoadStoredDetails(this.updatePID);
            }
            else
            {
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile defaultImage = await InstallationFolder.GetFileAsync("Assets\\DefaultProfilePic.jpg");
                decodedImage = await ImageMethods.ConvertStorageFileToBase64String(defaultImage);
            }
        }

        private async void LoadStoredDetails(string pid)
        {
            try
            {
                string query = "SELECT * FROM Patient WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    profileFirstName.Text = statement.Columns["FirstName"];
                    profileLastName.Text = statement.Columns["LastName"];
                    profileBloodGroup.SelectedIndex = profileBloodGroup.Items.IndexOf(statement.Columns["BloodGroup"]);
                    profileSexType.SelectedIndex = profileSexType.Items.IndexOf(statement.Columns["Sex"]);
                    string[] date = statement.Columns["Birthday"].Split('-');
                    profileDayComboBox.SelectedIndex = profileDayComboBox.Items.IndexOf(Int32.Parse(date[2]));
                    profileMonthComboBox.SelectedIndex = profileMonthComboBox.Items.IndexOf(date[1]);
                    profileYearComboBox.SelectedIndex = profileYearComboBox.Items.IndexOf(Int32.Parse(date[0]));
                    decodedImage = statement.Columns["Image"];
                    profilePic.Source = await ImageMethods.Base64StringToBitmap(decodedImage);
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM Address WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    profileAddress.Text = statement.Columns["Street"];
                    profileZip.Text = statement.Columns["ZIP"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---ADDRESS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    profileCity.Text = statement.Columns["City"];
                    this.pastGridGroup = statement.Columns["City"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---ADDRESS_ZIP" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressCity WHERE City = @city";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@city", profileCity.Text);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    profileState.Text = statement.Columns["State"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS-ADDRESS_CITY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string query = "SELECT * FROM AddressState WHERE State = @state";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@state", profileState.Text);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    profileCountry.Text = statement.Columns["Country"];
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---ADDRESS_STATE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string queryDetails = "SELECT * FROM MutableDetails WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(queryDetails);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();

                if (await statement.StepAsync())
                {
                    profileContactNumber.Text = statement.Columns["Mobile"];

                    if (statement.Columns["EmMobile"].ToString() != "0")
                    {
                        profileEmergencyNumber.Text = statement.Columns["EmMobile"];
                    }
                    else
                    {
                        profileEmergencyNumber.Text = "";
                    }

                    profileEmailAddress.Text = statement.Columns["Email"];
                    profileOccupation.Text = statement.Columns["Occupation"];
                    profileFamilyHistory.Text = statement.Columns["FamilyBackground"];

                    if (statement.Columns["Married"].Equals("T"))
                    {
                        profileMarried.IsChecked = true;
                    }
                    else
                    {
                        profileMarried.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---MUTABLE_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string allergyDetails = "SELECT * FROM MutableDetailsAllergy WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(allergyDetails);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();
                while (await statement.StepAsync())
                {
                    profileAllergies.Text += statement.Columns["Allergy"] + ",";
                }
                if (!profileAllergies.Text.Equals(""))
                {
                    profileAllergies.Text = profileAllergies.Text.Substring(0, profileAllergies.Text.Length - 1);
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---MUTABLE_DETAILS_ALLERGY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string operationDetails = "SELECT * FROM MutableDetailsOperation WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(operationDetails);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();

                while (await statement.StepAsync())
                {
                    profileOperations.Text += statement.Columns["Operation"] + ",";
                }
                if (!profileOperations.Text.Equals(""))
                {
                    profileOperations.Text = profileOperations.Text.Substring(0, profileOperations.Text.Length - 1);
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---MUTABLE_DETAILS_OPERATION" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string addictionDetails = "SELECT * FROM MutableDetailsAddiction WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(addictionDetails);
                statement.BindTextParameterWithName("@pid", pid);
                statement.EnableColumnsProperty();

                while (await statement.StepAsync())
                {
                    profileAddictions.Text += statement.Columns["Addiction"] + ",";
                }
                if (!profileAddictions.Text.Equals(""))
                {
                    profileAddictions.Text = profileAddictions.Text.Substring(0, profileAddictions.Text.Length - 1);
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---LOAD_STORED_DETAILS---MUTABLE_DETAILS_ADDICTION" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private async void SaveNewProfile(object sender, RoutedEventArgs e)
        {
            if (this.CheckIfFilled())
            {
                if (isUpdating)
                {
                    await this.UpdateBasicDetails();
                    await this.UpdateAddress(this.updatePID);
                    await this.UpdateMutableDetails(this.updatePID);
                    await this.UpdateGridView(this.updatePID);
                    this.isUpdating = false;
                }
                else
                {
                    string pid = await this.InsertBasicDetais();
                    await this.InsertAddress(pid);
                    await this.InsertMutableDetails(pid);
                    await this.InsertIntoGridView(pid, false);
                }

                this.navigationHelper.GoBack();

                if (profileProgressRing.IsActive == true)
                {
                    profileProgressRing.IsActive = false;
                    profileProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            else
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog("Please complete the form correctly before saving it.", "Error!");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                var dialogResult = await messageDialog.ShowAsync();
            }
        }

        private async Task<int> InsertIntoGridView(string pid, bool isUpdating)
        {
            try
            {
                BitmapImage bmp = await ImageMethods.Base64StringToBitmap(decodedImage);
                bool groupExist = false;

                IEnumerable<SampleDataGroup> samples = HomePageDataSoure._sampleDataSource.Groups;

                foreach (SampleDataGroup sample in samples)
                {
                    if (sample.Title.Equals(profileCity.Text.ToString()))
                    {
                        sample.Items.Add(new SampleDataItem(pid, profileFirstName.Text + " " + profileLastName.Text, profileAddress.Text, bmp));
                        groupExist = true;
                    }
                }
                if (!groupExist)
                {
                    SampleDataGroup group = new SampleDataGroup(profileCity.Text, profileCity.Text);
                    group.Items.Add(new SampleDataItem(pid, profileFirstName.Text + " " + profileLastName.Text, profileAddress.Text, bmp));
                    HomePageDataSoure._sampleDataSource.Groups.Add(group);
                }

                bool married;
                if (profileMarried.IsChecked.Value)
                {
                    married = true;
                }
                else
                {
                    married = false;
                }

                //This is used to see whether we are updating the Grid view or inserting into it. Incase we are updating then we don't need to 
                //add new element in the grid view as this would create two same elements.
                if (!isUpdating)
                {
                    await AnalysisPageDataSoure.GetItemsAsync();

                    AnalysisSampleDataItem temp = new AnalysisSampleDataItem(pid, profileFirstName.Text + " " + profileLastName.Text, profileBloodGroup.SelectedItem.ToString(), profileSexType.SelectedItem.ToString().ToCharArray()[0], married, bmp, profileOccupation.Text, profileFamilyHistory.Text, null, null, null, profileCity.Text, profileState.Text, profileCountry.Text, null);
                    AnalysisPageDataSoure._sampleDataSource.Groups.Add(temp);
                }

                return DBConnect.RESULT_OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_INTO_GRID_VIEW" + ex.Message);
                return DBConnect.RESULT_ERROR;
            }
        }

        private async Task<int> UpdateGridView(string pid)
        {
            try
            {
                BitmapImage bmp = await ImageMethods.Base64StringToBitmap(decodedImage);

                //This updates Grid View of NGO Home
                SampleDataItem updateItem = await HomePageDataSoure.GetItemAsync(pid);
                if (pastGridGroup != null)
                {
                    if (pastGridGroup.Equals(profileCity))
                    {
                        updateItem.Title = profileFirstName.Text + " " + profileLastName.Text;
                        updateItem.Description = profileAddress.Text;
                        updateItem.Image = bmp;
                    }
                    else
                    {
                        await HomePageDataSoure.DelItemAsync(pid);
                        //is update is given to update all the grid views. 
                        await this.InsertIntoGridView(pid, isUpdating);
                    }
                }

                //This updates Grid View of Analysis Page
                AnalysisSampleDataItem updateAnalysisItem = await AnalysisPageDataSoure.GetItemAsync(pid);
                if (profileBloodGroup.SelectedItem != null)
                {
                    updateAnalysisItem.BloodGroup = profileBloodGroup.Items[profileBloodGroup.SelectedIndex].ToString();
                }
                updateAnalysisItem.City = profileCity.Text;
                updateAnalysisItem.State = profileState.Text;
                updateAnalysisItem.Country = profileCountry.Text;
                updateAnalysisItem.Sex = profileSexType.SelectedItem.ToString().ToCharArray()[0];
                updateAnalysisItem.Name = profileFirstName.Text + " " + profileLastName.Text;
                updateAnalysisItem.Occupation = profileOccupation.Text;
                if (profileMarried.IsChecked.Value)
                {
                    updateAnalysisItem.Married = true;
                }
                else
                {
                    updateAnalysisItem.Married = false;
                }
                updateAnalysisItem.Image = bmp;
                updateAnalysisItem.FamilyBG = profileFamilyHistory.Text;

                return DBConnect.RESULT_OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_INTO_GRID_VIEW" + ex.Message);
                return DBConnect.RESULT_ERROR;
            }
        }

        private async Task<bool> UpdateMutableDetails(string pid)
        {
            try
            {
                string updateQuery = "UPDATE MutableDetails SET TimeStamp = @ts, Married = @married , Occupation = @occupation , FamilyBackground = @fb , Email = @email , Mobile = @mob , EmMobile = @eMob " +
                                     "WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(updateQuery);
                statement.BindTextParameterWithName("@pid", pid);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                if (profileMarried.IsChecked.Value)
                {
                    statement.BindTextParameterWithName("@married", "T");
                }
                else
                {
                    statement.BindTextParameterWithName("@married", "F");
                }
                statement.BindTextParameterWithName("@occupation", ExtraModules.removesemicolon(profileOccupation.Text.ToString()));
                statement.BindTextParameterWithName("@email", ExtraModules.removesemicolon(profileEmailAddress.Text.ToString()));
                statement.BindInt64ParameterWithName("@mob", Int64.Parse(profileContactNumber.Text.ToString()));
                if (!profileEmergencyNumber.Text.ToString().Equals(""))
                {
                    statement.BindInt64ParameterWithName("@eMob", Int64.Parse(profileEmergencyNumber.Text.ToString()));
                }
                else
                {
                    statement.BindInt64ParameterWithName("@eMob", 0000000000);
                }

                statement.BindTextParameterWithName("@fb", ExtraModules.removesemicolon(profileFamilyHistory.Text.ToString()));

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string deleteOperation = "DELETE FROM MutableDetailsOperation WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(deleteOperation);
                statement.BindTextParameterWithName("@pid", pid);
                await statement.StepAsync();

                statement.Reset();
                string deleteAllergy = "DELETE FROM MutableDetailsAllergy WHERE PID = @pid";
                statement = await this.database.PrepareStatementAsync(deleteAllergy);
                statement.BindTextParameterWithName("@pid", pid);
                await statement.StepAsync();

                statement.Reset();
                string deleteAddiction = "DELETE FROM MutableDetailsAddiction WHERE PID = @pid";
                statement = await this.database.PrepareStatementAsync(deleteAddiction);
                statement.BindTextParameterWithName("@pid", pid);
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS---DELETE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileAllergies.Text.Equals(""))
                {
                    string insertAllergyString = "INSERT INTO MutableDetailsAllergy (TimeStamp, PID, Allergy) VALUES (@ts, @pid, @allergy)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileAllergies.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertAllergyString);
                        statement.BindTextParameterWithName("@pid", pid);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@allergy", ExtraModules.removesemicolon(str));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false);
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS---ALLERGY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileAddictions.Text.Equals(""))
                {
                    string insertAddictionString = "INSERT INTO MutableDetailsAddiction (TimeStamp, PID, Addiction) VALUES (@ts, @pid, @addiction)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileAddictions.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertAddictionString);
                        statement.BindTextParameterWithName("@pid", pid);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@addiction", ExtraModules.removesemicolon(str));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false);
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS---ADDICTION" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileOperations.Text.ToString().Equals(""))
                {
                    string insertOperationString = "INSERT INTO MutableDetailsOperation (TimeStamp, PID, Operation) VALUES (@ts, @pid, @operation)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileOperations.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertOperationString);
                        statement.BindTextParameterWithName("@pid", pid);
                        statement.BindTextParameterWithName("@operation", str);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@operation", ExtraModules.removesemicolon(str));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false); 
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_MUTABLE_DETAILS---OPERATION" + "\n" + ex.Message + "\n" + result.ToString());
            }

            return true;
        }

        private async Task<bool> UpdateAddress(string pid)
        {
            try
            {
                string updateQuery = "UPDATE Address SET TimeStamp = @ts, ZIP = @zip , Street = @street WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(updateQuery);
                statement.BindTextParameterWithName("@pid", pid);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.BindTextParameterWithName("@street", ExtraModules.removesemicolon(profileAddress.Text.ToString()));

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_ADDRESS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string queryZIP = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
                Statement statement = await this.database.PrepareStatementAsync(queryZIP);
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));

                if (!await statement.StepAsync())
                {
                    statement.Reset();
                    string insertCityQuery = "INSERT INTO AddressZIP (TimeStamp, ZIP, City) VALUES (@ts, @zip, @city)";
                    statement = await this.database.PrepareStatementAsync(insertCityQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                    statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));

                    await statement.StepAsync();
                }
                else
                {
                    statement.Reset();
                    string updateCityQuery = "UPDATE AddressZIP SET TimeStamp = @ts, City = @city WHERE ZIP = @zip";
                    statement = await this.database.PrepareStatementAsync(updateCityQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                    statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));

                    await statement.StepAsync();
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_ADDRESS---ZIP" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string queryCity = "SELECT * FROM AddressCity WHERE City = @city";
                Statement statement = await this.database.PrepareStatementAsync(queryCity);
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());

                if (!await statement.StepAsync())
                {
                    string insertStateQuery = "INSERT INTO AddressCity (TimeStamp, City, State) VALUES (@ts, @city, @state)";
                    statement = await this.database.PrepareStatementAsync(insertStateQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));
                    statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));

                    await statement.StepAsync();
                }
                else
                {
                    statement.Reset();
                    string updateStateQuery = "UPDATE AddressCity SET TimeStamp = @ts, State = @state WHERE City = @city";
                    statement = await this.database.PrepareStatementAsync(updateStateQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));
                    statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));

                    await statement.StepAsync();
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_ADDRESS---City" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string queryState = "SELECT * FROM AddressState WHERE State = @state";
                Statement statement = await this.database.PrepareStatementAsync(queryState);
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());

                if (!await statement.StepAsync())
                {
                    string insertCountryQuery = "INSERT INTO AddressState (TimeStamp, State, Country) VALUES (@ts, @state, @country)";
                    statement = await this.database.PrepareStatementAsync(insertCountryQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));
                    statement.BindTextParameterWithName("@country", ExtraModules.removesemicolon(profileCountry.Text.ToString()));

                    await statement.StepAsync();
                }
                else
                {
                    statement.Reset();
                    string updateCountryQuery = "UPDATE AddressState SET TimeStamp = @ts, Country = @country WHERE State = @state";
                    statement = await this.database.PrepareStatementAsync(updateCountryQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));
                    statement.BindTextParameterWithName("@country", ExtraModules.removesemicolon(profileCountry.Text.ToString()));

                    await statement.StepAsync();
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_ADDRESS---STATE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            return true;
        }

        private async Task<bool> UpdateBasicDetails()
        {
            profileMainGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            profileProgressRing.IsActive = true;
            profileProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                string updateQuery = "UPDATE Patient SET TimeStamp = @ts, FirstName = @fName , LastName = @lName , BloodGroup = @bg , Sex = @sex , Birthday = @bday , Image = @image WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(updateQuery);
                statement.BindTextParameterWithName("@pid", this.updatePID);
                statement.BindTextParameterWithName("@fName", profileFirstName.Text);
                statement.BindTextParameterWithName("@lName", profileLastName.Text);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindTextParameterWithName("@bg", profileBloodGroup.Items[profileBloodGroup.SelectedIndex].ToString());
                statement.BindTextParameterWithName("@sex", profileSexType.Items[profileSexType.SelectedIndex].ToString());
                statement.BindTextParameterWithName("@image", decodedImage);
                statement.BindTextParameterWithName("@bday", profileYearComboBox.Items[profileYearComboBox.SelectedIndex].ToString() + "-" +
                                                             profileMonthComboBox.Items[profileMonthComboBox.SelectedIndex].ToString() + "-" +
                                                             profileDayComboBox.Items[profileDayComboBox.SelectedIndex].ToString());
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---UPDATE_BASIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            return true;
        }

        private async Task<string> InsertBasicDetais()
        {
            profileMainGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            profileProgressRing.IsActive = true;
            profileProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                string insertQuery = "INSERT INTO Patient (PID, TimeStamp, FirstName, LastName, BloodGroup, Sex, Birthday, Image) VALUES (@pid, @ts, @fName, @lName, @bg, @sex, @bday, @image)";
                Statement statement = await this.database.PrepareStatementAsync(insertQuery);

                /*THIS IS THE ONLY REGION WHERE WE ARE SUPPOSED TO ENTER THE PID OF THE FORM - TIMESTAMP~MAC*/
                /*HENCE CHANGE THE PID FROM ONLY DATE TIME TO DATETIME + MAC*/
                statement.BindTextParameterWithName("@pid", DateTime.Now.ToString());
                statement.BindTextParameterWithName("@fName", profileFirstName.Text);
                statement.BindTextParameterWithName("@lName", profileLastName.Text);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindTextParameterWithName("@bg", profileBloodGroup.Items[profileBloodGroup.SelectedIndex].ToString());
                statement.BindTextParameterWithName("@sex", profileSexType.Items[profileSexType.SelectedIndex].ToString());
                statement.BindTextParameterWithName("@image", decodedImage);
                statement.BindTextParameterWithName("@bday", profileYearComboBox.Items[profileYearComboBox.SelectedIndex].ToString() + "-" +
                                                             profileMonthComboBox.Items[profileMonthComboBox.SelectedIndex].ToString() + "-" +
                                                             profileDayComboBox.Items[profileDayComboBox.SelectedIndex].ToString());
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_BASIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string getPIDquery = "SELECT * FROM Patient";
                Statement statement = await this.database.PrepareStatementAsync(getPIDquery);
                statement.EnableColumnsProperty();
                string pid = "0";
                while (await statement.StepAsync())
                {
                    pid = (statement.Columns["PID"]);
                }

                return pid;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_BASIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());

                return "ERROR";
            }
        }

        private async Task<bool> InsertAddress(string PID)
        {
            try
            {
                string insertQuery = "INSERT INTO Address (TimeStamp, PID, ZIP, Street) VALUES (@ts, @pid, @zip, @street)";
                Statement statement = await this.database.PrepareStatementAsync(insertQuery);
                statement.BindTextParameterWithName("@pid", PID);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.BindTextParameterWithName("@street", ExtraModules.removesemicolon(profileAddress.Text.ToString()));

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_ADDRESS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string insertCityQuery = "INSERT INTO AddressZIP (TimeStamp, ZIP, City) VALUES (@ts, @zip, @city)";
                Statement statement = await this.database.PrepareStatementAsync(insertCityQuery);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_ADDRESS---ZIP" + "\n" + ex.Message + "\n" + result.ToString());
            }

            if (!profileState.Text.Equals(""))
            {
                try
                {
                    string insertStateQuery = "INSERT INTO AddressCity (TimeStamp, City, State) VALUES (@ts, @city, @state)";
                    Statement statement = await this.database.PrepareStatementAsync(insertStateQuery);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@city", ExtraModules.removesemicolon(profileCity.Text.ToString()));
                    statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));

                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_ADDRESS---CITY" + "\n" + ex.Message + "\n" + result.ToString());
                }

                if (!profileCountry.Text.Equals(""))
                {
                    try
                    {
                        string insertCountryQuery = "INSERT INTO AddressState (TimeStamp, State, Country) VALUES (@ts, @state, @country)";
                        Statement statement = await this.database.PrepareStatementAsync(insertCountryQuery);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@state", ExtraModules.removesemicolon(profileState.Text.ToString()));
                        statement.BindTextParameterWithName("@country", ExtraModules.removesemicolon(profileCountry.Text.ToString()));

                        await statement.StepAsync();
                    }
                    catch (Exception ex)
                    {
                        var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                        Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_ADDRESS---COUNTRY" + "\n" + ex.Message + "\n" + result.ToString());
                    }
                }
            }
            return true;
        }

        private async Task<bool> InsertMutableDetails(string PID)
        {
            try
            {
                string insertQuery = "INSERT INTO MutableDetails (TimeStamp, PID, Married, Occupation, FamilyBackground, Email, Mobile, EmMobile) " +
                                     "VALUES (@ts, @pid, @married, @occupation, @fb, @email, @mob, @eMob)";
                Statement statement = await this.database.PrepareStatementAsync(insertQuery);
                statement.BindTextParameterWithName("@pid", PID);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                if (profileMarried.IsChecked.Value)
                {
                    statement.BindTextParameterWithName("@married", "T");
                }
                else
                {
                    statement.BindTextParameterWithName("@married", "F");
                }
                statement.BindTextParameterWithName("@occupation", ExtraModules.removesemicolon(profileOccupation.Text.ToString()));
                statement.BindTextParameterWithName("@email", ExtraModules.removesemicolon(profileEmailAddress.Text.ToString()));
                statement.BindInt64ParameterWithName("@mob", Int64.Parse(profileContactNumber.Text.ToString()));
                if (!profileEmergencyNumber.Text.Equals(""))
                {
                    statement.BindInt64ParameterWithName("@eMob", Int64.Parse(profileEmergencyNumber.Text.ToString()));
                }
                else
                {
                    statement.BindInt64ParameterWithName("@eMob", 0);
                }
                statement.BindTextParameterWithName("@fb", ExtraModules.removesemicolon(profileFamilyHistory.Text.ToString()));

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_MUTABLE_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileAllergies.Text.Equals(""))
                {
                    string insertAllergyString = "INSERT INTO MutableDetailsAllergy (TimeStamp, PID, Allergy) VALUES (@ts, @pid, @allergy)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileAllergies.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertAllergyString);
                        statement.BindTextParameterWithName("@pid", PID);
                        statement.BindTextParameterWithName("@allergy", str);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false);
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_MUTABLE_DETAILS---ALLERGY" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileAddictions.Text.Equals(""))
                {
                    string insertAddictionString = "INSERT INTO MutableDetailsAddiction (TimeStamp, PID, Addiction) VALUES (@ts, @pid, @addiction)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileAddictions.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertAddictionString);
                        statement.BindTextParameterWithName("@pid", PID);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@addiction", ExtraModules.removesemicolon(str));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false);
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_MUTABLE_DETAILS---ADDICTION" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                if (!profileOperations.Text.ToString().Equals(""))
                {
                    string insertOperationString = "INSERT INTO MutableDetailsOperation (TimeStamp, PID, Operation) VALUES (@ts, @pid, @operation)";

                    foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(profileOperations.Text.ToString())).Split(','))
                    {
                        //Debug.WriteLine(str);
                        Statement statement = await this.database.PrepareStatementAsync(insertOperationString);
                        statement.BindTextParameterWithName("@pid", PID);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@operation", ExtraModules.removesemicolon(str));
                        //await statement.StepAsync().AsTask().ConfigureAwait(false); 
                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_PROFILE_FORM---INSERT_MUTABLE_DETAILS---OPERATION" + "\n" + ex.Message + "\n" + result.ToString());
            }
            return true;
        }

        private async void ResetNewProfile(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to reset? All the details entered would be lost.", "Confirmation.");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes", null));
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("No", null));
            messageDialog.DefaultCommandIndex = 1;
            var dialogResult = await messageDialog.ShowAsync();

            if (dialogResult.Label.Equals("Yes"))
            {
                profileFirstName.Text = "";
                profileLastName.Text = "";
                profileBloodGroup.SelectedItem = null;
                profileSexType.SelectedItem = null;
                profileMonthComboBox.SelectedItem = null;
                profileYearComboBox.SelectedItem = null;
                profileDayComboBox.SelectedItem = null;
                profileAddress.Text = "";
                profileCity.Text = "";
                profileZip.Text = "";
                profileState.Text = "";
                profileCountry.Text = "";
                profileContactNumber.Text = "";
                profileEmergencyNumber.Text = "";
                profileEmailAddress.Text = "";
                profileAllergies.Text = "";
                profileOccupation.Text = "";
                profileOperations.Text = "";
                profileFamilyHistory.Text = "";
                profileAddictions.Text = "";
                profileMarried.IsChecked = false;

            }

        }

        private async void GoBackNewProfile(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to cancel? All the details entered would be lost.", "Confirmation.");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes", null));
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("No", null));
            messageDialog.DefaultCommandIndex = 1;
            var dialogResult = await messageDialog.ShowAsync();

            if (dialogResult.Label.Equals("Yes"))
            {
                this.NavigationHelper.GoBack();
            }
        }

        private async void BrowseImage(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await file.OpenReadAsync();
                BitmapImage bmp = new BitmapImage();
                await bmp.SetSourceAsync(stream);
                profilePic.Source = bmp;
                decodedImage = await ImageMethods.ConvertStorageFileToBase64String(file);
            }
        }

        async private void CameraImage(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI cameraUI = new CameraCaptureUI();

            Windows.Storage.StorageFile capturedMedia =
                await cameraUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (capturedMedia != null)
            {
                var stream = await capturedMedia.OpenAsync(FileAccessMode.Read);
                BitmapImage bmp = new BitmapImage();
                await bmp.SetSourceAsync(stream);
                profilePic.Source = bmp;
                decodedImage = await ImageMethods.ConvertStorageFileToBase64String(capturedMedia);
            }
        }

        private void numberValidation(object sender, KeyRoutedEventArgs e)
        {
            if (((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
          && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9) || ((uint)e.Key >= (uint)Windows.System.VirtualKey.NumberPad0 && (uint)e.Key <= (uint)Windows.System.VirtualKey.NumberPad9) || (uint)e.Key == (uint)Windows.System.VirtualKey.Tab)
            {
                e.Handled = false;
            }
            else e.Handled = true;
        }

        //private void commaKeyDown(object sender, KeyRoutedEventArgs e)
        //{
        //    if (counterComma >= 1 && ((uint)e.Key == 188))
        //    {
        //        e.Handled = true;
        //    }
        //    else
        //    {
        //        e.Handled = false;
        //    }

        //    if (((uint)e.Key == 188))
        //    {
        //        counterComma++;
        //    }
        //    else
        //    {
        //        counterComma = 0;
        //    }

        //}

        private bool CheckIfFilled()
        {
            profileLastName.ClearValue(BorderBrushProperty);
            profileSexType.ClearValue(BorderBrushProperty);
            profileDayComboBox.ClearValue(BorderBrushProperty);
            profileMonthComboBox.ClearValue(BorderBrushProperty);
            profileYearComboBox.ClearValue(BorderBrushProperty);
            profileAddress.ClearValue(BorderBrushProperty);
            profileCity.ClearValue(BorderBrushProperty);
            profileZip.ClearValue(BorderBrushProperty);
            profileContactNumber.ClearValue(BorderBrushProperty);
            profileEmailAddress.ClearValue(BorderBrushProperty);
            profileOccupation.ClearValue(BorderBrushProperty);

            string dob = profileDayComboBox.SelectedItem.ToString() + "-" + (profileMonthComboBox.SelectedIndex + 1).ToString() + "-" + profileYearComboBox.SelectedItem.ToString();
            string pattern = "d-M-yyyy";
            DateTime DOB;
            Debug.WriteLine("Date: " + dob);
            if (!DateTime.TryParseExact(dob, pattern, null, System.Globalization.DateTimeStyles.None, out DOB))
            {
                profileDayComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                profileMonthComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                profileYearComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            Int32 catchInt32;
            Int64 catchInt64;
            if (profileFirstName.Text.Equals("") || profileLastName.Text.Equals("") || profileAddress.Text.Equals("") || profileCountry.Text.Equals("") ||
                profileState.Text.Equals("") || profileCity.Text.Equals("") || profileZip.Text.Equals("") || profileContactNumber.Text.Equals("") ||
                profileEmailAddress.Equals("") || profileOccupation.Text.Equals("") || profileSexType.SelectedItem == null || profileDayComboBox.SelectedItem == null || profileYearComboBox.SelectedItem == null || profileMonthComboBox.SelectedItem == null
                || !ExtraModules.isEmail(profileEmailAddress.Text) || !Int32.TryParse(profileZip.Text, out catchInt32) || !Int64.TryParse(profileContactNumber.Text, out catchInt64) || (!profileEmergencyNumber.Text.Equals("") && !Int64.TryParse(profileEmergencyNumber.Text, out catchInt64))
                || profileBloodGroup.SelectedItem == null)
            {
                if (profileFirstName.Text.Equals(""))
                {

                    profileFirstName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileLastName.Text.Equals(""))
                {
                    profileLastName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileBloodGroup.SelectedItem == null)
                {
                    profileBloodGroup.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileSexType.SelectedItem == null)
                {
                    profileSexType.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileDayComboBox.SelectedItem == null)
                {
                    profileDayComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileMonthComboBox.SelectedItem == null)
                {
                    profileMonthComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileYearComboBox.SelectedItem == null)
                {
                    profileYearComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileAddress.Text.Equals(""))
                {
                    profileAddress.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileCity.Text.Equals(""))
                {
                    profileCity.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileZip.Text.Equals("") || !Int32.TryParse(profileZip.Text, out catchInt32))
                {
                    profileZip.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileContactNumber.Text.Equals("") || !Int64.TryParse(profileContactNumber.Text, out catchInt64))
                {
                    profileContactNumber.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (!profileEmergencyNumber.Text.Equals("") && !Int64.TryParse(profileEmergencyNumber.Text, out catchInt64))
                {
                    profileEmergencyNumber.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileState.Text.Equals(""))
                {
                    profileState.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileCountry.Text.Equals(""))
                {
                    profileCountry.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileEmailAddress.Text.Equals(""))
                {
                    profileEmailAddress.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    profileEmailAddress.PlaceholderText = "";
                }
                else
                {
                    if (!ExtraModules.isEmail(profileEmailAddress.Text))
                    {
                        profileEmailAddress.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                        profileEmailAddress.PlaceholderText = "Invalid Email";
                        profileEmailAddress.Text = "";
                    }
                }
                if (profileOccupation.Text.Equals(""))
                {
                    profileOccupation.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }

                return false;
            }
            else
            {
                return true;
            }
        }
        //private void navigateBack(object sender, KeyRoutedEventArgs e)
        //{
        //    if ((uint)e.Key == (uint)Windows.System.VirtualKey.Back)
        //    {
        //        NavigationHelper.GoBack();
        //    }
        //}
    }
}
