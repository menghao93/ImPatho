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
    public sealed partial class UniversalSearchPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public List<AnalysisSampleDataItem> mainItemList, resultList;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

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

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            ShowProgress();
            var sample = await AnalysisPageDataSoure.GetItemsAsync();
            HideProgress();
            gridViewSource.Source = sample;
            mainItemList = RecordGrid.Items.OfType<AnalysisSampleDataItem>().ToList();
            RecordGrid.SelectedItem = null;
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

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
            bool addedFlag = false;
            resultList = new List<AnalysisSampleDataItem>();
            String searchQuery = UniversalSearchBox.Text;
            if (!searchQuery.Equals(""))
            {
                foreach (AnalysisSampleDataItem item in mainItemList)
                {
                    if (item.Name.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                        continue;
                    }
                    if (item.City.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                        continue;
                    }

                    if (item.State.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                        continue;
                    }

                    if (item.Occupation.ToLower().Contains(searchQuery))
                    {
                        resultList.Add(item);
                        continue;
                    }

                    foreach (string disease in item.Diseases.Values)
                    {
                        if (disease.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                            addedFlag = true;
                            break;
                        }
                    }

                    if (addedFlag)
                    {
                        continue;
                    }

                    foreach (string allergy in item.Allergy)
                    {
                        if (allergy.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                            addedFlag = true;
                            break;
                        }
                    }
                    if (addedFlag)
                    {
                        continue;
                    }

                    foreach (string addiction in item.Addiction)
                    {
                        if (addiction.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                            addedFlag = true;
                            break;
                        }
                    }
                    if (addedFlag)
                    {
                        continue;
                    }

                    foreach (string vaccine in item.Vaccines.Values)
                    {
                        if (vaccine.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                            addedFlag = true;
                            break;
                        }
                    }
                    if (addedFlag)
                    {
                        continue;
                    }

                    foreach (string operation in item.Operation)
                    {
                        if (operation.ToLower().Contains(searchQuery))
                        {
                            resultList.Add(item);
                            addedFlag = true;
                            break;
                        }
                    }
                    if (addedFlag)
                    {
                        continue;
                    }

                }
                gridViewSource.Source = resultList;
                UniversalSearchBox.Focus(FocusState.Keyboard);
                RecordGrid.SelectedItem = null;
            }
            HideProgress();
        }

        private void UniversalResetClicked(object sender, RoutedEventArgs e)
        {
            ShowProgress();
            gridViewSource.Source = mainItemList;
            UniversalSearchBox.Text = "";
            UniversalSearchBox.Focus(FocusState.Keyboard);
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
