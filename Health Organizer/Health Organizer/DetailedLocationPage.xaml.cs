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
using Health_Organizer.Data;
using System.Diagnostics;
using SQLiteWinRT;
using Health_Organizer.Database_Connet_Classes;

namespace Health_Organizer
{
    public sealed partial class DetailedLocationPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private DBConnect connection;
        private Database database;
        private int PID = -1;
        bool justLanded = true;
        Statement statement;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get
            {
                return this.navigationHelper;
            }
        }

        public DetailedLocationPage()
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            var group = await HomePageDataSoure.GetGroupAsync((String)e.Parameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;
            pageTitle.Text = group.Title;

            itemGridView.SelectedItem = null;
            this.disableAppButtons();
            this.InitializeDB();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void detailLocGridClicked(object sender, ItemClickEventArgs e)
        {
            SampleDataItem clickedItem = e.ClickedItem as SampleDataItem;
            this.PID = Int32.Parse(clickedItem.UniqueId);

            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateNewVisit), PID.ToString());
            }
        }

        private void LocationPageNewItemClicked(object sender, SelectionChangedEventArgs e)
        {
            if (itemGridView.SelectedItem != null && !justLanded)
            {
                this.enableAppButtons();
                SampleDataItem clickedItem = itemGridView.SelectedItem as SampleDataItem;
                this.PID = Int32.Parse(clickedItem.UniqueId);
                LocationPageCmdbar.IsOpen = true;
                this.enableAppButtons();
            }
            else
            {
                this.disableAppButtons();
                justLanded = false;
            }
        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage), this.PID.ToString());
            }
        }

        private void ProfileDetailsEditClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateProfileForm), this.PID.ToString());
            }
        }

        private void disableAppButtons()
        {
            LocationPageViewProfile.IsEnabled = false;
            LocationPageEditBut.IsEnabled = false;
            LocationPageDelBut.IsEnabled = false;
        }

        private void enableAppButtons()
        {
            LocationPageViewProfile.IsEnabled = true;
            LocationPageEditBut.IsEnabled = true;
            LocationPageDelBut.IsEnabled = true;
        }

        private async void InitializeDB()
        {
            this.connection = new DBConnect();
            await this.connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
            database = this.connection.GetConnection();
        }

        private async void ProfileDeleteButClicked(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems != null && this.PID > 0)
            {
                try
                {
                    string deleteQuery = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MedicalDetailsVaccine" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MedicalDetailsMedicine WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MedicalDetailsMedicine" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MedicalDetails WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MedicalDetails" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM Address WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---Address" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MutableDetailsOperation WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MutableDetailsOperation" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MutableDetailsAddiction WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MutableDetailsAddiction" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MutableDetailsAllergy WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MutableDetailsAllergy" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM MutableDetails WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---MutableDetails" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    statement.Reset();
                    string deleteQuery = "DELETE FROM Patient WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindIntParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---Patient" + "\n" + ex.Message + "\n" + result.ToString());
                }
            }

        }
    }
}
