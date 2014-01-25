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
        private int PID = -1;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        bool justLanded = true;

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

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            RecordProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
            RecordProgressRing.IsActive = true;
            var sample = await HomePageDataSoure.GetLimitedGroupsAsync();
            RecordProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            RecordProgressRing.IsActive = false;
            //this.DefaultViewModel["Groups"] = sample;
            groupedItemsViewSource.Source = sample;

            RecordGrid.SelectedItem = null;
            this.disableAppButtons();

            (SemanticZoomGrid.ZoomedOutView as ListViewBase).ItemsSource = groupedItemsViewSource.View.CollectionGroups;
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
            this.PID = Int32.Parse(((SampleDataItem)e.ClickedItem).UniqueId);

            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateNewVisit), ((SampleDataItem)e.ClickedItem).UniqueId);
            }
        }

        private void RecordPageNewItemClicked(object sender, SelectionChangedEventArgs e)
        {
            if (RecordGrid.SelectedItem != null && !justLanded)
            {
                this.enableAppButtons();
                SampleDataItem clickedItem = RecordGrid.SelectedItem as SampleDataItem;
                this.PID = Int32.Parse(clickedItem.UniqueId);
                RecordPageCmdbar.IsOpen = true;
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

        private void ProfileDetailsEditBut(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CreateProfileForm), this.PID.ToString());
            }
        }

        private async void recordGridHeader(object sender, RoutedEventArgs e)
        {
            TextBlock clickedItem = ((e.OriginalSource as Button).Content as StackPanel).Children[0] as TextBlock;

            if (clickedItem.Text.ToString() != "")
            {
                IEnumerable<SampleDataGroup> samples = await HomePageDataSoure.GetGroupsAsync();
                foreach (SampleDataGroup sample in samples)
                {
                    if (sample.Title.Equals(clickedItem.Text.ToString()))
                    {
                        if (this.Frame != null)
                        {
                            this.Frame.Navigate(typeof(DetailedLocationPage), sample.UniqueId);
                        }
                    }
                }
            }
        }

        private void disableAppButtons()
        {
            RecordPageViewProfile.IsEnabled = false;
            RecordPageEditBut.IsEnabled = false;
        }

        private void enableAppButtons()
        {
            RecordPageViewProfile.IsEnabled = true;
            RecordPageEditBut.IsEnabled = true;
        }

        private void SemanticZoomButClicked(object sender, RoutedEventArgs e)
        {
            SemanticZoomGrid.ToggleActiveView();
            RecordPageCmdbar.IsOpen = false;
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
