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

            this.InitializeDB();
            this.InitializeComponents();
            //this.queryDB();
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
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

        private void CameraImage(object sender, RoutedEventArgs e)
        {

        }

        private async void SaveNewProfile(object sender, RoutedEventArgs e)
        {
            if (this.CheckIfFilled())
            {
                try
                {
                    await database.ExecuteStatementAsync("BEGIN TRANSACTION");
                    int pid = await this.insertBasicDetais();
                    this.insertAddress(pid);
                    this.insertMutableDetails(pid);
                    await database.ExecuteStatementAsync("COMMIT TRANSACTION");

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

                foreach (string str in profileAllergies.Text.ToString().Split(','))
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
                string insertAllergyString = "INSERT INTO MutableDetailsAddiction (PID, Addiction) VALUES (@pid, @addiction)";
                statement = await this.database.PrepareStatementAsync(insertAllergyString);

                foreach (string str in profileAddictions.Text.ToString().Split(','))
                {
                    Debug.WriteLine(str);
                    statement = await this.database.PrepareStatementAsync(insertAllergyString);
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

                foreach (string str in profileAddictions.Text.ToString().Split(','))
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

            if (profileFirstName.Text.Equals("") || profileLastName.Text.Equals("") || profileAddress.Text.Equals("") ||
                profileCity.Text.Equals("") || profileZip.Text.Equals("") || profileContactNumber.Text.Equals("") ||
                profileEmailAddress.Equals("") || profileOccupation.Text.Equals("") || profileSexType.SelectedItem == null || profileDayComboBox.SelectedItem == null || profileYearComboBox.SelectedItem == null || profileMonthComboBox.SelectedItem == null
                || (!Regex.IsMatch(profileEmailAddress.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")))
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

        private async void CancelNewProfile(object sender, RoutedEventArgs e)
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
            if (counterComma >= 1 && ((uint)e.Key == 188)){
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
    }
}
