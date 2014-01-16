using Health_Organizer.Data;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml.Controls;
using System.Resources;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using SQLiteWinRT;
using System.Diagnostics;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Health_Organizer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainMenuPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Database database;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainMenuPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this.loadDescriptionStrings();
        }

        private void loadDescriptionStrings()
        {
            //outputBlock.Text += String.Format("\nThe current culture is {0}.\n", CultureInfo.CurrentCulture.Name);
            ResourceLoader rm = new ResourceLoader();
            string description = rm.GetString("DESCRIPTION_TEST");
            bloodTestDes.Text = description;
            description = rm.GetString("DESCRIPTION_DOC_KIT");
            docKitDes.Text = description;
            description = rm.GetString("DESCRIPTION_SURVEY");
            recordSurveyDes.Text = description;
            description = rm.GetString("DESCRIPTION_ANALYSIS");
            analysisDes.Text = description;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void test_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(DiseaseTestPage));
            }
        }

        private void doctor_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(DocKitPage));
            }
        }

        private void health_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RecordPage));
            }
        }

        private void analysis_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(AnalysisPage));
            }
        }

        private void UniversalSearchClick(object sender, RoutedEventArgs e)
        {
            MainMenuGridAnimation.Begin();
        }

        private void MainPageAnimationCompleted(object sender, object e)
        {
            if (this.Frame != null)
            {

                this.Frame.Navigate(typeof(UniversalSearchPage));
            }
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {

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

        public void sendToServer(string TimeStamp)
        {
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
                    columnNames += statement.Columns["name"] + ",";
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
