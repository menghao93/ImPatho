using Health_Organizer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer
{
    public sealed partial class RecordPage : Page
    {
        private int PID = 1;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public RecordPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var sample = await HomePageDataSoure.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = sample;
            recordGrid.SelectedItem = null;
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void AddNewEntryForm(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                //This indicates that no PID is passed as a parameter
                this.Frame.Navigate(typeof(CreateProfileForm), "-1");
            }
        }

        private void recordGridViewClicked(object sender, ItemClickEventArgs e)
        {
            SampleDataItem clickedItem = e.ClickedItem as SampleDataItem;
            this.PID = Int32.Parse(clickedItem.UniqueId);

            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateNewVisit), PID.ToString());
            }
        }

        private async void recordGridHeader(object sender, RoutedEventArgs e)
        {
            TextBlock clickedItem = ((e.OriginalSource as Button).Content as StackPanel).Children[0] as TextBlock;
            IEnumerable<SampleDataGroup> samples = await HomePageDataSoure.GetGroupsAsync();
            foreach (SampleDataGroup sample in samples) {
                if (sample.Title.Equals(clickedItem.Text.ToString())) {
                    if (this.Frame != null)
                    {
                        this.Frame.Navigate(typeof(DetailedLocationPage), sample.UniqueId);
                    }   
                }
            }
        }
    }
}
