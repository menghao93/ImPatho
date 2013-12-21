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

namespace Health_Organizer
{
    public sealed partial class DetailedLocationPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private int PID = -1;
        bool justLanded = true;
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

        private void profileDetailsEditBut(object sender, RoutedEventArgs e)
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
    }
}
