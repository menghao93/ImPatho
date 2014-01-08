using Health_Organizer.Data;
using Health_Organizer.Data_Model_Classes;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Health_Organizer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class UniversalSearchPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public List<AnalysisSampleDataItem> mainItemList, resultList;
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


        public UniversalSearchPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            ShowProgress();
            var sample = await AnalysisPageDataSoure.GetItemsAsync();
            HideProgress();
            gridViewSource.Source = sample;
            mainItemList = RecordGrid.Items.OfType<AnalysisSampleDataItem>().ToList();
            RecordGrid.SelectedItem = null;
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
            UniversalPageGridAnimation.Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void UniversalSearchClicked(object sender, RoutedEventArgs e)
        {
            ShowProgress();
            resultList = new List<AnalysisSampleDataItem>();
            String searchQuery = UniversalSearchBox.Text;
            if (!searchQuery.Equals(""))
            {
                foreach (AnalysisSampleDataItem item in mainItemList)
                {
                    if (item.Name.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                    }
                    if (item.City.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                    }

                    if (item.State.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                    }

                    if (item.Occupation.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                    }
                    foreach (string disease in item.Diseases.Values)
                    {
                        if (disease.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                        }
                    }

                    foreach (string allergy in item.Allergy)
                    {
                        if (allergy.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                        }
                    }

                    foreach (string addiction in item.Addiction)
                    {
                        if (addiction.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                        }
                    }

                    foreach (string vaccine in item.Vaccines.Values)
                    {
                        if (vaccine.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                        }
                    }

                    foreach (string operation in item.Operation)
                    {
                        if (operation.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                        }
                    }

                }
                gridViewSource.Source = resultList;
            }
            HideProgress();
        }

        private void UniversalResetClicked(object sender, RoutedEventArgs e)
        {
            ShowProgress();
            gridViewSource.Source = mainItemList;
            HideProgress();
            RecordGrid.SelectedItem = null;
        }
        public void ShowProgress()
        {
            UniversalProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
            UniversalProgressRing.IsActive = true;
        }
        public void HideProgress()
        {
            UniversalProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UniversalProgressRing.IsActive = false;
        }
    }
}
