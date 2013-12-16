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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Health_Organizer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CreateNewVisit : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

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


        public CreateNewVisit()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            //Clear all the text field in form before displaying it
            this.ClearAllFields();

            //Adding days and years to combobox in form
            for (int i = 0; i < 31; i++)
            {
                visitDayComboBox.Items.Add(i + 1);
            }

            for (int i = 2000; i <= DateTime.Now.Year; i++)
            {
                visitYearComboBox.Items.Add(i);
            }

            //Set current date in form
            visitDayComboBox.SelectedItem = DateTime.Now.Day;
            visitMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            visitYearComboBox.SelectedItem = DateTime.Now.Year;
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



        private void ClearAllFields()
        {
            VisitSymptoms.Text = "";
            VisitDiseasesDiagnosed.Text = "";
            VisitMedicineGiven.Text = "";
            VisitBloodGlucose.Text = "";
            VisitSystolicBP.Text = "";
            VisitDiastolicBP.Text = "";
        }

        private void AddVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;
        }

        private void EditVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;

        }

        private void DeleteVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;
        }

        private void VisitSaveClicked(object sender, RoutedEventArgs e)
        {
            VisitFormBar.IsOpen = false;
        }

        private void VisitCancelClicked(object sender, RoutedEventArgs e)
        {
            this.ClearAllFields();
            VisitFormBar.IsOpen = false;
        }
    }
}
