using Health_Organizer.Common;
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

namespace Health_Organizer
{
    public sealed partial class CreateProfileForm : Page
    {
        private DBConnect connection;
        private string decodedImage = null;
        private Database database;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        int counterComma = 0;
        private bool isUpdating = false;
        private int updatePID = -1;

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

            this.InitializeComponents();
        }

        private async void InitializeComponents()
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

            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile defaultImage = await InstallationFolder.GetFileAsync("Assets\\DefaultProfilePic.jpg");
            decodedImage = await ImageMethods.ConvertStorageFileToBase64String(defaultImage);
        }

        private async void InitializeDB()
        {
            this.connection = new DBConnect();
            await this.connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
            database = this.connection.GetConnection();

            if (isUpdating)
            {
                this.LoadStoredDetails(this.updatePID);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.InitializeDB();
            if (Int32.Parse(e.Parameter as string) != -1)
            {
                this.isUpdating = true;
                this.updatePID = Int32.Parse(e.Parameter as string);
            }
        }

        private async void LoadStoredDetails(int pid)
        {
            string query = "SELECT * FROM Patient WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindInt64ParameterWithName("@pid", pid);
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
            statement.Reset();

            query = "SELECT * FROM Address WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                profileAddress.Text = statement.Columns["Street"];
                profileZip.Text = statement.Columns["ZIP"];
            }
            statement.Reset();

            query = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                profileCity.Text = statement.Columns["City"];
            }
            statement.Reset();

            query = "SELECT * FROM AddressCity WHERE City = @city";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindTextParameterWithName("@city", profileCity.Text);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                profileState.Text = statement.Columns["State"];
            }
            statement.Reset();

            query = "SELECT * FROM AddressState WHERE State = @state";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindTextParameterWithName("@state", profileState.Text);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                profileCountry.Text = statement.Columns["Country"];
            }

            statement.Reset();
            string queryDetails = "SELECT * FROM MutableDetails WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(queryDetails);
            statement.BindIntParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();

            if (await statement.StepAsync())
            {
                profileContactNumber.Text = statement.Columns["Mobile"];
                profileEmergencyNumber.Text = statement.Columns["EmMobile"];
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

            string allergyDetails = "SELECT * FROM MutableDetailsAllergy WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(allergyDetails);
            statement.BindIntParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                profileAllergies.Text += statement.Columns["Allergy"] + ",";
            }
            if (!profileAllergies.Text.Equals(""))
            {
                profileAllergies.Text = profileAllergies.Text.Substring(0, profileAllergies.Text.Length - 1);
            }
            statement.Reset();


            string operationDetails = "SELECT * FROM MutableDetailsOperation WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(operationDetails);
            statement.BindIntParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();

            while (await statement.StepAsync())
            {
                profileOperations.Text += statement.Columns["Operation"] + ",";
            }
            if (!profileOperations.Text.Equals(""))
            {
                profileOperations.Text = profileOperations.Text.Substring(0, profileOperations.Text.Length - 1);
            }
            statement.Reset();

            string addictionDetails = "SELECT * FROM MutableDetailsAddiction WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(addictionDetails);
            statement.BindIntParameterWithName("@pid", pid);
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
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

        private async void SaveNewProfile(object sender, RoutedEventArgs e)
        {
            if (this.CheckIfFilled())
            {
                try
                {
                    if (isUpdating)
                    {
                        this.UpdateBasicDetails();
                        this.UpdateAddress(this.updatePID);
                        this.UpdateMutableDetails(this.updatePID);
                        this.isUpdating = false;
                    }
                    else
                    {
                        await database.ExecuteStatementAsync("BEGIN TRANSACTION");
                        int pid = await this.insertBasicDetais();
                        this.insertAddress(pid);
                        this.insertMutableDetails(pid);
                        await database.ExecuteStatementAsync("COMMIT TRANSACTION");
                    }
                    this.navigationHelper.GoBack();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog("Please complete the form before saving it.", "Error!");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                var dialogResult = await messageDialog.ShowAsync();
            }
        }

        private async void UpdateMutableDetails(int pid)
        {
            string updateQuery = "UPDATE MutableDetails SET Married = @married , Occupation = @occupation , FamilyBackground = @fb , Email = @email , Mobile = @mob , EmMobile = @eMob " +
                                 "WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(updateQuery);
            statement.BindIntParameterWithName("@pid", pid);
            if (profileMarried.IsChecked.Value)
            {
                statement.BindTextParameterWithName("@married", "T");
            }
            else
            {
                statement.BindTextParameterWithName("@married", "F");
            }
            statement.BindTextParameterWithName("@occupation", profileOccupation.Text.ToString());
            statement.BindTextParameterWithName("@email", profileEmailAddress.Text.ToString());
            statement.BindInt64ParameterWithName("@mob", Int64.Parse(profileContactNumber.Text.ToString()));
            statement.BindInt64ParameterWithName("@eMob", Int64.Parse(profileEmergencyNumber.Text.ToString()));
            statement.BindTextParameterWithName("@fb", profileFamilyHistory.Text.ToString());

            await statement.StepAsync();

            statement.Reset();
            string deleteOperation = "DELETE FROM MutableDetailsOperation WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(deleteOperation);
            statement.BindIntParameterWithName("@pid", pid);
            await statement.StepAsync();

            statement.Reset();
            string deleteAllergy = "DELETE FROM MutableDetailsAllergy WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(deleteAllergy);
            statement.BindIntParameterWithName("@pid", pid);
            await statement.StepAsync();

            statement.Reset();
            string deleteAddiction = "DELETE FROM MutableDetailsAddiction WHERE PID = @pid";
            statement = await this.database.PrepareStatementAsync(deleteAddiction);
            statement.BindIntParameterWithName("@pid", pid);
            await statement.StepAsync();

            statement.Reset();
            if (!profileAllergies.Text.Equals(""))
            {
                string insertAllergyString = "INSERT INTO MutableDetailsAllergy (PID, Allergy) VALUES (@pid, @allergy)";

                foreach (string str in profileAllergies.Text.ToString().Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertAllergyString);
                    statement.BindIntParameterWithName("@pid", pid);
                    statement.BindTextParameterWithName("@allergy", str);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false);
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            if (!profileAddictions.Text.Equals(""))
            {
                string insertAllergyString = "INSERT INTO MutableDetailsAddiction (PID, Addiction) VALUES (@pid, @addiction)";
                statement = await this.database.PrepareStatementAsync(insertAllergyString);

                foreach (string str in profileAddictions.Text.ToString().Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertAllergyString);
                    statement.BindTextParameterWithName("@addiction", str);
                    statement.BindIntParameterWithName("@pid", pid);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false);
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            if (!profileOperations.Text.ToString().Equals(""))
            {
                string insertOperationString = "INSERT INTO MutableDetailsOperation (PID, Operation) VALUES (@pid, @operation)";

                foreach (string str in profileAddictions.Text.ToString().Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertOperationString);
                    statement.BindIntParameterWithName("@pid", pid);
                    statement.BindTextParameterWithName("@operation", str);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false); 
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
        }

        private async void UpdateAddress(int pid)
        {
            string updateQuery = "UPDATE Address SET ZIP = @zip , Street = @street WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(updateQuery);
            statement.BindIntParameterWithName("@pid", pid);
            statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
            statement.BindTextParameterWithName("@street", profileAddress.Text.ToString());

            await statement.StepAsync();

            statement.Reset();
            string queryZIP = "SELECT * FROM AddressZIP WHERE ZIP = @zip";
            statement = await this.database.PrepareStatementAsync(queryZIP);
            statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));

            if (!await statement.StepAsync())
            {
                statement.Reset();
                string insertCityQuery = "INSERT INTO AddressZIP (ZIP, City) VALUES (@zip, @city)";
                statement = await this.database.PrepareStatementAsync(insertCityQuery);
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());

                await statement.StepAsync();
            }
            else
            {
                statement.Reset();
                string updateCityQuery = "UPDATE AddressZIP SET City = @city WHERE ZIP = @zip";
                statement = await this.database.PrepareStatementAsync(updateCityQuery);
                statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());

                await statement.StepAsync();
            }

            statement.Reset();
            string queryCity = "SELECT * FROM AddressCity WHERE City = @city";
            statement = await this.database.PrepareStatementAsync(queryCity);
            statement.BindTextParameterWithName("@city", profileCity.Text.ToString());

            if (!await statement.StepAsync())
            {
                string insertStateQuery = "INSERT INTO AddressCity (City, State) VALUES (@city, @state)";
                statement = await this.database.PrepareStatementAsync(insertStateQuery);
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());

                await statement.StepAsync();
            }
            else
            {
                statement.Reset();
                string updateStateQuery = "UPDATE AddressCity SET State = @state WHERE City = @city";
                statement = await this.database.PrepareStatementAsync(updateStateQuery);
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());

                await statement.StepAsync();
            }

            statement.Reset();
            string queryState = "SELECT * FROM AddressState WHERE State = @state";
            statement = await this.database.PrepareStatementAsync(queryState);
            statement.BindTextParameterWithName("@state", profileState.Text.ToString());

            if (!await statement.StepAsync())
            {
                string insertCountryQuery = "INSERT INTO AddressState (State, Country) VALUES (@state, @country)";
                statement = await this.database.PrepareStatementAsync(insertCountryQuery);
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());
                statement.BindTextParameterWithName("@country", profileCountry.Text.ToString());

                await statement.StepAsync();
            }
            else
            {
                statement.Reset();
                string updateCountryQuery = "UPDATE AddressState SET Country = @country WHERE State = @state";
                statement = await this.database.PrepareStatementAsync(updateCountryQuery);
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());
                statement.BindTextParameterWithName("@country", profileCountry.Text.ToString());

                await statement.StepAsync();
            }
        }

        private async void UpdateBasicDetails()
        {
            string updateQuery = "UPDATE Patient SET FirstName = @fName , LastName = @lName , BloodGroup = @bg , Sex = @sex , Birthday = @bday , Image = @image WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(updateQuery);
            statement.BindIntParameterWithName("@pid", this.updatePID);
            statement.BindTextParameterWithName("@fName", profileFirstName.Text);
            statement.BindTextParameterWithName("@lName", profileLastName.Text);
            statement.BindTextParameterWithName("@bg", profileBloodGroup.Items[profileBloodGroup.SelectedIndex].ToString());
            statement.BindTextParameterWithName("@sex", profileSexType.Items[profileSexType.SelectedIndex].ToString());
            statement.BindTextParameterWithName("@image", decodedImage);
            statement.BindTextParameterWithName("@bday", profileYearComboBox.Items[profileYearComboBox.SelectedIndex].ToString() + "-" +
                                                         profileMonthComboBox.Items[profileMonthComboBox.SelectedIndex].ToString() + "-" +
                                                         profileDayComboBox.Items[profileDayComboBox.SelectedIndex].ToString());
            await statement.StepAsync();
        }

        private async Task<int> insertBasicDetais()
        {
            string insertQuery = "INSERT INTO Patient (FirstName, LastName, BloodGroup, Sex, Birthday, Image) VALUES (@fName, @lName, @bg, @sex, @bday, @image)";
            Statement statement = await this.database.PrepareStatementAsync(insertQuery);
            statement.BindTextParameterWithName("@fName", profileFirstName.Text);
            statement.BindTextParameterWithName("@lName", profileLastName.Text);
            statement.BindTextParameterWithName("@bg", profileBloodGroup.Items[profileBloodGroup.SelectedIndex].ToString());
            statement.BindTextParameterWithName("@sex", profileSexType.Items[profileSexType.SelectedIndex].ToString());
            statement.BindTextParameterWithName("@image", decodedImage);
            statement.BindTextParameterWithName("@bday", profileYearComboBox.Items[profileYearComboBox.SelectedIndex].ToString() + "-" +
                                                         profileMonthComboBox.Items[profileMonthComboBox.SelectedIndex].ToString() + "-" +
                                                         profileDayComboBox.Items[profileDayComboBox.SelectedIndex].ToString());
            await statement.StepAsync();
            string getPIDquery = "SELECT * FROM Patient";
            statement = await this.database.PrepareStatementAsync(getPIDquery);
            statement.EnableColumnsProperty();
            int pid = 0;
            while (await statement.StepAsync())
            {
                pid = Int32.Parse(statement.Columns["PID"]);
            }
            return pid;
        }

        private async void insertAddress(int PID)
        {
            string insertQuery = "INSERT INTO Address (PID, ZIP, Street) VALUES (@pid, @zip, @street)";
            Statement statement = await this.database.PrepareStatementAsync(insertQuery);
            statement.BindIntParameterWithName("@pid", PID);
            statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
            statement.BindTextParameterWithName("@street", profileAddress.Text.ToString());

            await statement.StepAsync();

            string insertCityQuery = "INSERT INTO AddressZIP (ZIP, City) VALUES (@zip, @city)";
            statement = await this.database.PrepareStatementAsync(insertCityQuery);
            statement.BindIntParameterWithName("@zip", Int32.Parse(profileZip.Text.ToString()));
            statement.BindTextParameterWithName("@city", profileCity.Text.ToString());

            await statement.StepAsync();

            if (!profileState.Text.Equals(""))
            {
                string insertStateQuery = "INSERT INTO AddressCity (City, State) VALUES (@city, @state)";
                statement = await this.database.PrepareStatementAsync(insertStateQuery);
                statement.BindTextParameterWithName("@city", profileCity.Text.ToString());
                statement.BindTextParameterWithName("@state", profileState.Text.ToString());

                await statement.StepAsync();

                if (!profileCountry.Text.Equals(""))
                {
                    string insertCountryQuery = "INSERT INTO AddressState (State, Country) VALUES (@state, @country)";
                    statement = await this.database.PrepareStatementAsync(insertCountryQuery);
                    statement.BindTextParameterWithName("@state", profileState.Text.ToString());
                    statement.BindTextParameterWithName("@country", profileCountry.Text.ToString());

                    await statement.StepAsync();
                }
            }
        }

        private async void insertMutableDetails(int PID)
        {
            string insertQuery = "INSERT INTO MutableDetails (PID, Married, Occupation, FamilyBackground, Email, Mobile, EmMobile) " +
                                 "VALUES (@pid, @married, @occupation, @fb, @email, @mob, @eMob)";
            Statement statement = await this.database.PrepareStatementAsync(insertQuery);
            statement.BindIntParameterWithName("@pid", PID);
            if (profileMarried.IsChecked.Value)
            {
                statement.BindTextParameterWithName("@married", "T");
            }
            else
            {
                statement.BindTextParameterWithName("@married", "F");
            }
            statement.BindTextParameterWithName("@occupation", profileOccupation.Text.ToString());
            statement.BindTextParameterWithName("@email", profileEmailAddress.Text.ToString());
            statement.BindInt64ParameterWithName("@mob", Int64.Parse(profileContactNumber.Text.ToString()));
            if (!profileEmergencyNumber.Text.Equals(""))
            {
                statement.BindInt64ParameterWithName("@eMob", Int64.Parse(profileEmergencyNumber.Text.ToString()));
            }
            if (!profileFamilyHistory.Text.ToString().Equals(""))
            {
                statement.BindTextParameterWithName("@fb", profileFamilyHistory.Text.ToString());
            }

            await statement.StepAsync();

            if (!profileAllergies.Text.Equals(""))
            {
                string insertAllergyString = "INSERT INTO MutableDetailsAllergy (PID, Allergy) VALUES (@pid, @allergy)";

                foreach (string str in ExtraModules.RemoveExtraCommas(profileAllergies.Text.ToString()).Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertAllergyString);
                    statement.BindIntParameterWithName("@pid", PID);
                    statement.BindTextParameterWithName("@allergy", str);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false);
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            if (!profileAddictions.Text.Equals(""))
            {
                string insertAddictionString = "INSERT INTO MutableDetailsAddiction (PID, Addiction) VALUES (@pid, @addiction)";
                statement = await this.database.PrepareStatementAsync(insertAddictionString);

                foreach (string str in ExtraModules.RemoveExtraCommas(profileAddictions.Text.ToString()).Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertAddictionString);
                    statement.BindTextParameterWithName("@addiction", str);
                    statement.BindIntParameterWithName("@pid", PID);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false);
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            if (!profileOperations.Text.ToString().Equals(""))
            {
                string insertOperationString = "INSERT INTO MutableDetailsOperation (PID, Operation) VALUES (@pid, @operation)";

                foreach (string str in ExtraModules.RemoveExtraCommas(profileOperations.Text.ToString()).Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertOperationString);
                    statement.BindIntParameterWithName("@pid", PID);
                    statement.BindTextParameterWithName("@operation", str);
                    //await statement.StepAsync().AsTask().ConfigureAwait(false); 
                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            //this.queryDB();
        }

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

            if (profileFirstName.Text.Equals("") || profileLastName.Text.Equals("") || profileAddress.Text.Equals("") || profileCountry.Text.Equals("") ||
                profileState.Text.Equals("") || profileCity.Text.Equals("") || profileZip.Text.Equals("") || profileContactNumber.Text.Equals("") ||
                profileEmailAddress.Equals("") || profileOccupation.Text.Equals("") || profileSexType.SelectedItem == null || profileDayComboBox.SelectedItem == null || profileYearComboBox.SelectedItem == null || profileMonthComboBox.SelectedItem == null
                || (!Regex.IsMatch(profileEmailAddress.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
                || profileBloodGroup.SelectedItem == null)
            {
                Debug.WriteLine("false");
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
                if (profileZip.Text.Equals(""))
                {
                    profileZip.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (profileContactNumber.Text.Equals(""))
                {
                    profileContactNumber.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
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
                    if (!Regex.IsMatch(profileEmailAddress.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
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
                Debug.WriteLine("true");
                return true;
            }
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

        private void profileDayComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(profileDayComboBox.Items[profileDayComboBox.SelectedIndex]);
        }

        private void profileMonthComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void profileYearComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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

        private void commaKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (counterComma >= 1 && ((uint)e.Key == 188))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }

            if (((uint)e.Key == 188))
            {
                counterComma++;
            }
            else
            {
                counterComma = 0;
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
                database.Dispose();
                this.connection.CloseConnection(DBConnect.ORG_HOME_DB);
            }
        }
    }
}
