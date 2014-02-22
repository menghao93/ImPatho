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
using Health_Organizer.Data_Model_Classes;
using System.Text.RegularExpressions;

namespace Health_Organizer
{
    public sealed partial class DetailedLocationPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Database database;
        Statement statement;

        private string PID = "-1";
        bool justLanded = true;
        bool isSearchBarVisible = false;

        List<SampleDataItem> ListOfAllItem;

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

        public void updateListOfAllItem()
        {
            ListOfAllItem = itemGridView.Items.OfType<SampleDataItem>().ToList(); 
        }

        private void InitializeDB()
        {
            this.database = App.database;
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
            //this.DefaultViewModel["Group"] = group;
            //this.DefaultViewModel["Items"] = group.Items;
            gridViewSource.Source = group.Items;
            pageTitle.Text = group.Title;

            this.disableAppButtons();
            this.InitializeDB();
            this.updateListOfAllItem();
            itemGridView.SelectedItem = null;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void detailLocGridClicked(object sender, ItemClickEventArgs e)
        {
            SampleDataItem clickedItem = e.ClickedItem as SampleDataItem;
            this.PID = (clickedItem.UniqueId);

            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateNewVisit), PID);
            }
        }

        private void LocationPageNewItemClicked(object sender, SelectionChangedEventArgs e)
        {
            if ( ! isSearchBarVisible)
            {
                if (itemGridView.SelectedItem != null && !justLanded)
                {
                    this.enableAppButtons();
                    SampleDataItem clickedItem = itemGridView.SelectedItem as SampleDataItem;
                    this.PID = (clickedItem.UniqueId);
                    LocationPageCmdbar.IsOpen = true;
                    this.enableAppButtons();
                }
                else
                {
                    this.disableAppButtons();
                    justLanded = false;
                }
            }
            
        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage), this.PID);
            }
        }

        private void ProfileDetailsEditClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateProfileForm), this.PID);
            }
        }

        private async void ProfileDeleteButClicked(object sender, RoutedEventArgs e)
        {
            //if (itemGridView.SelectedItems != null && this.PID > 0)
            if (itemGridView.SelectedItems != null)
            {
                try
                {
                    string deleteQuery = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid";
                    statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
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
                    statement.BindTextParameterWithName("@pid", this.PID);
                    await statement.StepAsync();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("DETAILED_LOCATION_PAGE---PROFILE_DELETE_BUT_CLICKED---Patient" + "\n" + ex.Message + "\n" + result.ToString());
                }

                await HomePageDataSoure.DelItemAsync(PID.ToString());
                await AnalysisPageDataSoure.DelItemAsync(PID.ToString());
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


        private List<SampleDataItem> Search(string searchString)
        {
            var regex = new Regex(searchString, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            List<SampleDataItem> list = ListOfAllItem;
            List<SampleDataItem> resultlist = new List<SampleDataItem>();

            foreach (SampleDataItem item in list)
            {
                if (regex.IsMatch(item.ToString())){
                    resultlist.Add(item);
                }
            }

            return resultlist;
        }


        private void SearchBoxQueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            if (! args.QueryText.ToString().Equals(""))
            {
                this.gridViewSource.Source = Search(args.QueryText.ToString()); 
                //this.DefaultViewModel["Items"] = Search(args.QueryText.ToString());                
            }
            else
            {
                this.gridViewSource.Source = ListOfAllItem;
                //this.DefaultViewModel["Items"] = ListOfAllItem;
            }

            itemGridView.SelectedItem = null;
        }

        private void LocationSearchButClicked(object sender, RoutedEventArgs e)
        {
            if (isSearchBarVisible)
            {
                LocationPageSearchBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                isSearchBarVisible = false;
            }
            else
            {
                LocationPageSearchBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                isSearchBarVisible = true;
            }
        }

        private void LocationPageItemClicked(object sender, PointerRoutedEventArgs e)
        {
            LocationPageSearchBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            isSearchBarVisible = false;
        }
        private void navigateBack(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Back)
            {
                NavigationHelper.GoBack();
            }
        }
    }
}
