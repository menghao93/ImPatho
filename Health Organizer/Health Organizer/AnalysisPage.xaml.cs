using Health_Organizer.Data;
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
using Health_Organizer.Data_Model_Classes;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using System.Collections.ObjectModel;
using De.TorstenMandelkow.MetroChart;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;

namespace Health_Organizer
{
    public sealed partial class AnalysisPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public List<AnalysisSampleDataItem> mainItemList;
        public static List<AnalysisSampleDataItem> resultList;
        public static List<string> cityList;
        public static List<string> diseaseList;
        public static List<string> allergyList;
        public static List<string> addictionList;
        public static List<string> vaccinationList;
        public static List<string> operationList;
        public static Dictionary<string, string> city2state;

        bool ByDateFlag, CityFlag, StateFlag, SexFlag, StatusFlag, BGFlag, DiseaseFlag, AllergyFlag,
            AddictionFlag, VaccineFlag, OperationFlag;

        Int16 sexMale = 0;
        Int16 isMarried = 0;
        private TestPageViewModel testPageObject;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public AnalysisPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var sample = await AnalysisPageDataSoure.GetItemsAsync();
            gridViewSource.Source = sample;
            mainItemList = RecordGrid.Items.OfType<AnalysisSampleDataItem>().ToList();
            resultList = mainItemList;
            fillAllLists();
            fillAllComboBox();

            this.AnalysisResetBox();
            this.AnalysisResetFlag();
            this.AnalysisResetDateBox();
            this.disableCommandBarButtons();

            this.DataContext = new TestPageViewModel();
            (this.DataContext as TestPageViewModel).UpdateGraphView();

            RecordGrid.SelectedItem = null;
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

        private void AnalysisResetFieldsClicked(object sender, RoutedEventArgs e)
        {
            this.AnalysisResetBox();
            this.AnalysisResetFlag();
            this.AnalysisResetDateBox();
            this.AnalysisDateBoxDisable();
            gridViewSource.Source = mainItemList;
            this.UpdateView();
            (this.DataContext as TestPageViewModel).UpdateGraphView();
            RecordGrid.SelectedItem = null;
        }

        private void AnalysisSearchClicked(object sender, RoutedEventArgs e)
        {
            this.AnalysisValidateFields();
            this.AnalysisSetFlags();
            this.UpdateView();
            RecordGrid.SelectedItem = null;

            (this.DataContext as TestPageViewModel).UpdateGraphView();
            //AnalysisGraphGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //AnalysisDetailsGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        //public void AddNewGraph(ObservableCollection<TestClass> a)
        //{
        //    De.TorstenMandelkow.MetroChart.PieChart p = new De.TorstenMandelkow.MetroChart.PieChart();
        //    p.Height = 400;
        //    p.Width = 400;
        //    p.Margin = new Thickness(10, 0, 0, 0);
        //    AnalysisGraphStackPanel.Children.Add(p);
        //    ChartSeries c = new ChartSeries();
        //    c.ItemsSource = temp;
        //    c.SeriesTitle = "temp";
        //    c.DisplayMember = "Category";
        //    c.ValueMember = "Number";
        //    p.Series.Add(c);
        //}
        private void AnalysisItemClicked(object sender, ItemClickEventArgs e)
        {
            AnalysisSampleDataItem clickedItem = e.ClickedItem as AnalysisSampleDataItem;

            if (this.Frame != null && clickedItem != null)
            {
                this.Frame.Navigate(typeof(CreateNewVisit), clickedItem.UniqueId);
            }
        }

        private async void AnalysisExportListClicked(object sender, RoutedEventArgs e)
        {
            this.AnalysisValidateFields();
            this.AnalysisSetFlags();
            this.UpdateView();
            RecordGrid.SelectedItem = null;

            FileSavePicker savePicker = new FileSavePicker();

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as

            savePicker.FileTypeChoices.Add("Tabular Data", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace

            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();


            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                string data = "";
                string columnSeparator = ", ";

                data += "Name,  " + "Blood Group,    " + "Sex,   " + "Martial Status,   " + "Occupation,    " + "City,  " + "State,    " + "Addiction, " + "Operation, " + "Vaccination,   " + "Family Background" + "\r\n";

                foreach (AnalysisSampleDataItem item in resultList)
                {
                    data += item.Name + columnSeparator;
                    data += item.BloodGroup + columnSeparator;
                    data += item.Sex + columnSeparator;
                    data += ExtraModules.getMartialStatus(item.Married) + columnSeparator;
                    data += item.Occupation + columnSeparator;
                    data += item.City + columnSeparator;
                    data += item.State + columnSeparator;

                    foreach (string addiction in item.Addiction)
                    {
                        data += addiction + "; ";
                    }
                    data += columnSeparator;

                    foreach (string operation in item.Operation)
                    {
                        data += operation + "; ";
                    }
                    data += columnSeparator;

                    foreach (string vaccine in item.Vaccines.Values)
                    {
                        data += vaccine + "; ";
                    }
                    data += columnSeparator;

                    data += item.FamilyBG.Replace(",", "; ") + columnSeparator;

                    data += "\r\n";
                }

                // write to file
                await FileIO.WriteTextAsync(file, data);

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == FileUpdateStatus.Complete)
                {
                    Debug.WriteLine("File " + file.Name + " was saved.");
                }
                else
                {
                    Debug.WriteLine("File " + file.Name + " couldn't be saved.");
                }
            }
        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            AnalysisSampleDataItem selectedItem = RecordGrid.SelectedItem as AnalysisSampleDataItem;

            if (this.Frame != null && selectedItem != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage), selectedItem.UniqueId);
            }

        }

        private void ShareProfileClicked(object sender, RoutedEventArgs e)
        {
            EmailInfoForm.IsOpen = true;
            AnalysisCustomDialogAnimation.Begin();
        }

        private async void ExportProfileClicked(object sender, RoutedEventArgs e)
        {
            AnalysisSampleDataItem selectedItem = RecordGrid.SelectedItem as AnalysisSampleDataItem;

            FileSavePicker savePicker = new FileSavePicker();

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as

            savePicker.FileTypeChoices.Add("Tabular Data", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace

            savePicker.SuggestedFileName = selectedItem.Name;

            StorageFile file = await savePicker.PickSaveFileAsync();


            if (file != null && selectedItem != null)
            {
                CachedFileManager.DeferUpdates(file);

                // write to file
                await FileIO.WriteTextAsync(file, ExtraModules.getFileDataForAnalysisItem(selectedItem));

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == FileUpdateStatus.Complete)
                {
                    Debug.WriteLine("File " + file.Name + " was saved.");
                }
                else
                {
                    Debug.WriteLine("File " + file.Name + " couldn't be saved.");
                }
            }
        }

        private async void SendMailClicked(object sender, RoutedEventArgs e)
        {
            if (isEmailInfoFilled())
            {
                AnalysisSampleDataItem selectedItem = RecordGrid.SelectedItem as AnalysisSampleDataItem;

                if (selectedItem != null)
                {
                    Windows.Storage.StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
                    StorageFile sampleFile = await temporaryFolder.CreateFileAsync(selectedItem.Name.Trim() + ".csv", CreationCollisionOption.ReplaceExisting);

                    EmailInfoForm.IsOpen = false;

                    await FileIO.WriteTextAsync(sampleFile, ExtraModules.getFileDataForAnalysisItem(selectedItem));

                    await ExtraModules.Send_Email(FromEmail.Text, FromPassword.Password, ToEmail.Text, Subject.Text, "Check from Health Organiser", sampleFile.Path);

                    await sampleFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    this.clearEmailInfoFields();
                }
                else
                {
                    EmailInfoForm.IsOpen = false;
                }
            }
        }

        private void CancleMailClicked(object sender, RoutedEventArgs e)
        {
            EmailInfoForm.IsOpen = false;
            this.clearEmailInfoFields();
        }

        private void AnalysisCitySelected(object sender, SelectionChangedEventArgs e)
        {
            string state;
            if (AnalysisCityBox.SelectedIndex != -1)
            {
                if (city2state.TryGetValue(AnalysisCityBox.SelectedItem.ToString(), out state))
                {
                    AnalysisStateBox.SelectedItem = state;
                }
            }
        }

        private void AnalysisNewItemSelected(object sender, SelectionChangedEventArgs e)
        {
            AnalysisSampleDataItem selectedItem = RecordGrid.SelectedItem as AnalysisSampleDataItem;
            if (selectedItem != null)
            {
                AnalysisPageCmdbar.IsOpen = true;
                this.enableCommandBarButtons();
            }
            else
            {
                AnalysisPageCmdbar.IsOpen = false;
                this.disableCommandBarButtons();
            }

        }

        private void UpdateView()
        {
            resultList = new List<AnalysisSampleDataItem>();

            foreach (AnalysisSampleDataItem item in mainItemList)
            {
                if (ByDateFlag)
                {
                    this.checkSelectedDates();

                    int lastDateOn = item.DatesVisited.Count;

                    if (lastDateOn > 0)
                    {
                        DateTime lastDate = ExtraModules.ConvertStringToDateTime(item.DatesVisited.ElementAt(lastDateOn - 1));
                        DateTime fromDate = new DateTime(Convert.ToInt32(AnalysisFromYearComboBox.SelectedItem), AnalysisFromMonthComboBox.SelectedIndex + 1, Convert.ToInt32(AnalysisFromDayComboBox.SelectedItem));
                        DateTime toDate = new DateTime(Convert.ToInt32(AnalysisToYearComboBox.SelectedItem), AnalysisToMonthComboBox.SelectedIndex + 1, Convert.ToInt32(AnalysisToDayComboBox.SelectedItem));

                        if (!(fromDate <= lastDate && toDate >= lastDate))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                }


                if (CityFlag)
                {
                    if (!AnalysisCityBox.SelectedItem.ToString().Equals(item.City))
                    {
                        continue;
                    }
                }

                if (StateFlag)
                {
                    if (!AnalysisStateBox.SelectedItem.ToString().Equals(item.State))
                    {
                        continue;
                    }
                }

                if (SexFlag)
                {
                    switch (sexMale)
                    {
                        case 1: if (item.Sex != 'M')
                            {
                                continue;
                            }
                            break;
                        case -1: if (item.Sex != 'F')
                            {
                                continue;
                            }
                            break;
                    }
                }

                if (StatusFlag)
                {
                    switch (isMarried)
                    {
                        case 1: if (!item.Married)
                            {
                                continue;
                            }
                            break;
                        case -1: if (item.Married)
                            {
                                continue;
                            }
                            break;
                    }
                }

                if (BGFlag)
                {
                    if (!AnalysisBloodGroupBox.SelectedItem.ToString().Equals(item.BloodGroup))
                    {
                        continue;
                    }
                }

                if (DiseaseFlag)
                {
                    bool found = false;
                    foreach (string disease in item.Diseases.Values)
                    {
                        if (AnalysisDiseaseBox.SelectedItem.ToString().Equals(disease))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }

                if (AllergyFlag)
                {
                    bool found = false;
                    foreach (string allergy in item.Allergy)
                    {
                        if (AnalysisAllergyBox.SelectedItem.ToString().Equals(allergy))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }

                if (AddictionFlag)
                {
                    bool found = false;
                    foreach (string addiction in item.Addiction)
                    {
                        if (AnalysisAddictionBox.SelectedItem.ToString().Equals(addiction))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }

                if (VaccineFlag)
                {
                    bool found = false;
                    foreach (string vaccine in item.Vaccines.Values)
                    {
                        if (AnalysisVaccinationBox.SelectedItem.ToString().Equals(vaccine))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }

                if (OperationFlag)
                {
                    bool found = false;
                    foreach (string operation in item.Operation)
                    {
                        if (AnalysisOperationsBox.SelectedItem.ToString().Equals(operation))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }
                }

                resultList.Add(item);
            }
            gridViewSource.Source = resultList;
        }

        private void AnalysisAllChecked(object sender, RoutedEventArgs e)
        {
            sexMale = 0;
            AnalysisMaleCheck.IsChecked = false;
            AnalysisFemaleCheck.IsChecked = false;
        }

        private void AnalysisMaleChecked(object sender, RoutedEventArgs e)
        {
            sexMale = 1;
            AnalysisFemaleCheck.IsChecked = false;
            AnalysisAllCheck.IsChecked = false;
        }

        private void AnalysisFemaleChecked(object sender, RoutedEventArgs e)
        {
            sexMale = -1;
            AnalysisMaleCheck.IsChecked = false;
            AnalysisAllCheck.IsChecked = false;
        }

        private void AnalysisAllCatChecked(object sender, RoutedEventArgs e)
        {
            isMarried = 0;
            AnalysisMarriedCheck.IsChecked = false;
            AnalysisUnmarriedCheck.IsChecked = false;
        }

        private void AnalysisMarriedChecked(object sender, RoutedEventArgs e)
        {
            isMarried = 1;
            AnalysisUnmarriedCheck.IsChecked = false;
            AnalysisAllCatCheck.IsChecked = false;
        }

        private void AnalysisUnmarriedChecked(object sender, RoutedEventArgs e)
        {
            isMarried = -1;
            AnalysisMarriedCheck.IsChecked = false;
            AnalysisAllCatCheck.IsChecked = false;
        }

        private void AnalysisByDateChecked(object sender, RoutedEventArgs e)
        {
            ByDateFlag = true;
            this.AnalysisDateBoxEnable();
        }

        private void AnalysisByDateUnChecked(object sender, RoutedEventArgs e)
        {
            ByDateFlag = false;
            this.AnalysisDateBoxDisable();
        }

        private void AnalysisValidateFields()
        {
            this.checkSelectedDates();

            DateTime toDate = new DateTime(Convert.ToInt16(AnalysisToYearComboBox.SelectedItem), AnalysisToMonthComboBox.SelectedIndex + 1, Convert.ToInt16(AnalysisToDayComboBox.SelectedItem));
            DateTime fromDate = new DateTime(Convert.ToInt16(AnalysisFromYearComboBox.SelectedItem), AnalysisFromMonthComboBox.SelectedIndex + 1, Convert.ToInt16(AnalysisFromDayComboBox.SelectedItem));

            if (toDate < fromDate)
            {
                AnalysisFromDayComboBox.SelectedItem = AnalysisToDayComboBox.SelectedItem;
                AnalysisFromMonthComboBox.SelectedIndex = AnalysisToMonthComboBox.SelectedIndex;
                AnalysisFromYearComboBox.SelectedItem = AnalysisToYearComboBox.SelectedItem;
            }
        }

        private void fillAllLists()
        {
            cityList = new List<string>();
            city2state = new Dictionary<string, string>();
            diseaseList = new List<string>();
            allergyList = new List<string>();
            addictionList = new List<string>();
            vaccinationList = new List<string>();
            operationList = new List<string>();


            //Adding city and corrosponding State to Lists
            foreach (AnalysisSampleDataItem item in mainItemList)
            {
                if (!cityList.Contains(item.City))
                {
                    cityList.Add(item.City);

                    if (!city2state.ContainsValue(item.State))
                    {
                        city2state.Add(item.City, item.State);
                    }

                }

                //Adding disease to list
                foreach (string diseases in item.Diseases.Values)
                {
                    if (!diseaseList.Contains(diseases))
                    {
                        diseaseList.Add(diseases);
                    }
                }

                //Adding allergies to list
                foreach (string allergy in item.Allergy)
                {
                    if (!allergyList.Contains(allergy))
                    {
                        allergyList.Add(allergy);
                    }
                }

                //Adding addictions to list
                foreach (string addiction in item.Addiction)
                {
                    if (!addictionList.Contains(addiction))
                    {
                        addictionList.Add(addiction);
                    }
                }

                foreach (string vaccine in item.Vaccines.Values)
                {
                    if (!vaccinationList.Contains(vaccine))
                    {
                        vaccinationList.Add(vaccine);
                    }
                }

                foreach (string operation in item.Operation)
                {
                    if (!operationList.Contains(operation))
                    {
                        operationList.Add(operation);
                    }
                }

            }
        }

        private void fillAllComboBox()
        {
            AnalysisAllCheck.IsChecked = true;
            AnalysisAllCatCheck.IsChecked = true;

            for (Int16 i = 0; i < 31; i++)
            {
                AnalysisFromDayComboBox.Items.Add(i + 1);
                AnalysisToDayComboBox.Items.Add(i + 1);
            }

            for (Int16 i = 1980; i < DateTime.Now.Year; i++)
            {
                AnalysisFromYearComboBox.Items.Add(i + 1);
                AnalysisToYearComboBox.Items.Add(i + 1);
            }

            this.AnalysisResetDateBox();

            if (cityList.Count() > 0)
            {
                AnalysisCityBox.IsEnabled = true;
                foreach (string city in cityList)
                {
                    AnalysisCityBox.Items.Add(city);
                }
            }
            else
            {
                AnalysisCityBox.IsEnabled = false;
            }

            if (city2state.Values.Count() > 0)
            {
                AnalysisStateBox.IsEnabled = true;
                foreach (string state in city2state.Values)
                {
                    AnalysisStateBox.Items.Add(state);
                }
            }
            else
            {
                AnalysisStateBox.IsEnabled = false;
            }

            if (diseaseList.Count() > 0)
            {
                AnalysisDiseaseBox.IsEnabled = true;
                foreach (string disease in diseaseList)
                {
                    AnalysisDiseaseBox.Items.Add(disease);
                }
            }
            else
            {
                AnalysisDiseaseBox.IsEnabled = false;
            }


            if (allergyList.Count() > 0)
            {
                AnalysisAllergyBox.IsEnabled = true;
                foreach (string allergy in allergyList)
                {
                    AnalysisAllergyBox.Items.Add(allergy);
                }
            }
            else
            {
                AnalysisAllergyBox.IsEnabled = false;
            }

            if (addictionList.Count() > 0)
            {
                AnalysisAddictionBox.IsEnabled = true;
                foreach (string addiction in addictionList)
                {
                    AnalysisAddictionBox.Items.Add(addiction);
                }
            }
            else
            {
                AnalysisAddictionBox.IsEnabled = false;
            }

            if (vaccinationList.Count() > 0)
            {
                AnalysisVaccinationBox.IsEnabled = true;
                foreach (string vaccine in vaccinationList)
                {
                    AnalysisVaccinationBox.Items.Add(vaccine);
                }
            }
            else
            {
                AnalysisVaccinationBox.IsEnabled = false;
            }

            if (operationList.Count() > 0)
            {
                AnalysisOperationsBox.IsEnabled = true;
                foreach (string operation in operationList)
                {
                    AnalysisOperationsBox.Items.Add(operation);
                }
            }
            else
            {
                AnalysisOperationsBox.IsEnabled = false;
            }

        }

        private void AnalysisResetBox()
        {
            AnalysisCityBox.SelectedIndex = -1;
            AnalysisStateBox.SelectedIndex = -1;
            AnalysisByDate.IsChecked = false;
            AnalysisAllCheck.IsChecked = true;
            AnalysisAllCatCheck.IsChecked = true;
            AnalysisMaleCheck.IsChecked = false;
            AnalysisFemaleCheck.IsChecked = false;
            AnalysisMarriedCheck.IsChecked = false;
            AnalysisUnmarriedCheck.IsChecked = false;
            AnalysisBloodGroupBox.SelectedIndex = -1;
            AnalysisDiseaseBox.SelectedIndex = -1;
            AnalysisAllergyBox.SelectedIndex = -1;
            AnalysisAddictionBox.SelectedIndex = -1;
            AnalysisVaccinationBox.SelectedIndex = -1;
            AnalysisOperationsBox.SelectedIndex = -1;

            this.AnalysisDateBoxDisable();
        }

        private void AnalysisResetDateBox()
        {
            AnalysisFromDayComboBox.SelectedIndex = 0;
            AnalysisFromMonthComboBox.SelectedIndex = 0;
            AnalysisFromYearComboBox.SelectedIndex = 0;
            AnalysisToDayComboBox.SelectedItem = DateTime.Now.Day;
            AnalysisToMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            AnalysisToYearComboBox.SelectedItem = DateTime.Now.Year;
        }

        private void AnalysisResetFlag()
        {
            ByDateFlag = false;
            CityFlag = false;
            StateFlag = false;
            SexFlag = false;
            StatusFlag = false;
            BGFlag = false;
            DiseaseFlag = false;
            AllergyFlag = false;
            AddictionFlag = false;
            VaccineFlag = false;
            OperationFlag = false;
        }

        private void AnalysisSetFlags()
        {
            if (AnalysisCityBox.SelectedIndex != -1)
            {
                CityFlag = true;
            }
            if (AnalysisStateBox.SelectedIndex != -1)
            {
                StateFlag = true;
            }
            if (sexMale != 0)
            {
                SexFlag = true;
            }
            if (isMarried != 0)
            {
                StatusFlag = true;
            }
            if (AnalysisBloodGroupBox.SelectedIndex != -1)
            {
                BGFlag = true;
            }
            if (AnalysisDiseaseBox.SelectedIndex != -1)
            {
                DiseaseFlag = true;
            }
            if (AnalysisAllergyBox.SelectedIndex != -1)
            {
                AllergyFlag = true;
            }
            if (AnalysisAddictionBox.SelectedIndex != -1)
            {
                AddictionFlag = true;
            }
            if (AnalysisVaccinationBox.SelectedIndex != -1)
            {
                VaccineFlag = true;
            }
            if (AnalysisOperationsBox.SelectedIndex != -1)
            {
                OperationFlag = true;
            }
        }

        private void AnalysisDateBoxDisable()
        {
            AnalysisFromDayComboBox.IsEnabled = false;
            AnalysisFromMonthComboBox.IsEnabled = false;
            AnalysisFromYearComboBox.IsEnabled = false;
            AnalysisToDayComboBox.IsEnabled = false;
            AnalysisToMonthComboBox.IsEnabled = false;
            AnalysisToYearComboBox.IsEnabled = false;
        }

        private void AnalysisDateBoxEnable()
        {
            AnalysisFromDayComboBox.IsEnabled = true;
            AnalysisFromMonthComboBox.IsEnabled = true;
            AnalysisFromYearComboBox.IsEnabled = true;
            AnalysisToDayComboBox.IsEnabled = true;
            AnalysisToMonthComboBox.IsEnabled = true;
            AnalysisToYearComboBox.IsEnabled = true;
        }

        private void enableCommandBarButtons()
        {
            ViewProfile.IsEnabled = true;
            ShareProfile.IsEnabled = true;
            ExportProfile.IsEnabled = true;
        }

        private void disableCommandBarButtons()
        {
            ViewProfile.IsEnabled = false;
            ShareProfile.IsEnabled = false;
            ExportProfile.IsEnabled = false;
        }

        private void AnalysisDefaultOptionClicked(object sender, RoutedEventArgs e)
        {
            AnalysisGraphGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AnalysisDetailsGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AnalysisOptionsSubHeader.Text = "Default";
        }

        private void AnalysisGraphicalOptionClicked(object sender, RoutedEventArgs e)
        {
            AnalysisGraphGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AnalysisDetailsGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AnalysisOptionsSubHeader.Text = "Graphical";
        }


        //for data binding in graph
        public class TestPageViewModel
        {
            public ObservableCollection<TestClass> Sex { get; private set; }
            public ObservableCollection<TestClass> Married { get; private set; }
            public ObservableCollection<TestClass> City { get; private set; }
            public ObservableCollection<TestClass> Blood { get; private set; }
            public ObservableCollection<TestClass> Disease { get; private set; }
            public ObservableCollection<TestClass> Addiction { get; private set; }
            public ObservableCollection<TestClass> Operation { get; private set; }
            public ObservableCollection<TestClass> Allergy { get; private set; }
            public ObservableCollection<TestClass> Vaccine { get; private set; }
            public TestPageViewModel()
            {
                Sex = new ObservableCollection<TestClass>();
                Married = new ObservableCollection<TestClass>();
                Disease = new ObservableCollection<TestClass>();
                City = new ObservableCollection<TestClass>();
                Blood = new ObservableCollection<TestClass>();
                Addiction = new ObservableCollection<TestClass>();
                Allergy = new ObservableCollection<TestClass>();
                Operation = new ObservableCollection<TestClass>();
                Vaccine = new ObservableCollection<TestClass>();
            }

            public async void UpdateGraphView()
            {
                List<String> diseaseListRepeatedValues = new List<string>();
                List<String> allergyListRepeatedValues = new List<string>();
                List<String> addictionListRepeatedValues = new List<string>();
                List<String> vaccinationListRepeatedValues = new List<string>();
                List<String> operationListRepeatedValues = new List<string>();
                List<String> cityListRepeatedValues = new List<string>();

                //Sex = new ObservableCollection<TestClass>();
                //Married = new ObservableCollection<TestClass>();
                //Disease = new ObservableCollection<TestClass>();
                //City = new ObservableCollection<TestClass>();
                //Blood = new ObservableCollection<TestClass>();
                //Addiction = new ObservableCollection<TestClass>();
                //Allergy = new ObservableCollection<TestClass>();
                //Operation = new ObservableCollection<TestClass>();
                //Vaccine = new ObservableCollection<TestClass>();

                Sex.Clear();
                City.Clear();
                Married.Clear();
                Addiction.Clear();
                Allergy.Clear();
                Addiction.Clear();
                Operation.Clear();
                Vaccine.Clear();
                Disease.Clear();

                foreach (var i in resultList)
                {
                    diseaseListRepeatedValues.AddRange(i.Diseases.Values.ToList());
                    allergyListRepeatedValues.AddRange(i.Allergy);
                    operationListRepeatedValues.AddRange(i.Operation);
                    addictionListRepeatedValues.AddRange(i.Addiction);
                    vaccinationListRepeatedValues.AddRange(i.Vaccines.Values.ToList());
                    cityListRepeatedValues.Add(i.City);
                }


                //displaying sex pie chart
                int x = resultList.Count(i => i.Sex == 'M');
                window = CoreWindow.GetForCurrentThread();
                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    if (resultList.Count() != 0)
                    {
                        Sex.Add(new TestClass() { Category = "Male", Number = (int)Math.Round((double)x * 100 / resultList.Count()) });
                        Sex.Add(new TestClass() { Category = "Female", Number = (int)Math.Round((double)100 * (resultList.Count() - x) / resultList.Count()) });
                    }
                });

                //display married pie chart
                int y = resultList.Count(i => i.Married == true);
                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    if (resultList.Count() != 0)
                    {
                        Married.Add(new TestClass() { Category = "Married", Number = (int)Math.Round((double)y * 100 / resultList.Count()) });
                        Married.Add(new TestClass() { Category = "Unmarried", Number = (int)Math.Round(((double)resultList.Count() - y) * 100 / resultList.Count()) });
                    }
                });

                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display cities graph
                    foreach (var i in cityListRepeatedValues.Distinct())
                    {
                        City.Add(new TestClass() { Category = i, Number = cityListRepeatedValues.Count(j => j.Equals(i)) });
                    }
                });

                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display blood group chart
                    List<String> bloodgrp = new List<string>();
                    foreach (var i in resultList)
                    {
                        bloodgrp.Add(i.BloodGroup);
                    }
                    foreach (var i in bloodgrp.Distinct())
                    {
                        Blood.Add(new TestClass() { Category = i.ToString(), Number = resultList.Count(j => j.BloodGroup.Equals(i.ToString())) });
                    }
                });

                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display disease graph
                    foreach (var i in diseaseListRepeatedValues.Distinct())
                    {
                        Disease.Add(new TestClass() { Category = i.ToString(), Number = diseaseListRepeatedValues.Count(j => j.Equals(i)) });
                    }
                });
                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display addictions graph
                    foreach (var i in addictionListRepeatedValues.Distinct())
                    {
                        Addiction.Add(new TestClass() { Category = i.ToString(), Number = addictionListRepeatedValues.Count(j => j.Equals(i)) });
                    }
                });
                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display addictions graph
                    //display operations graph
                    foreach (var i in operationListRepeatedValues.Distinct())
                    {
                        Operation.Add(new TestClass() { Category = i.ToString(), Number = operationListRepeatedValues.Count(j => j.Equals(i.ToString())) });
                    }
                });

                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    foreach (var i in allergyListRepeatedValues.Distinct())
                    {
                        Allergy.Add(new TestClass() { Category = i.ToString(), Number = allergyListRepeatedValues.Count(j => j.Equals(i.ToString())) });
                    }
                });
                window.Dispatcher.RunAsync(CoreDispatcherPriority.Low, delegate
                {
                    //display vaccine graph
                    foreach (var i in vaccinationListRepeatedValues.Distinct())
                    {
                        Vaccine.Add(new TestClass() { Category = i.ToString(), Number = vaccinationListRepeatedValues.Count(j => j.Equals(i.ToString())) });
                    }
                });
            }

            private object selectedItem = null;
            private CoreWindow window;

            public object SelectedItem
            {
                get
                {
                    return selectedItem;
                }
                set
                {
                    // selected item has changed
                    selectedItem = value;
                }
            }
        }

        // class which represent a data point in the chart
        public class TestClass
        {
            public string Category { get; set; }

            public int Number { get; set; }
        }

        private bool isEmailInfoFilled()
        {
            bool flag = true;
            if (!ExtraModules.isEmail(FromEmail.Text))
            {
                FromEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                FromEmail.PlaceholderText = "Invalid Email";
                FromEmail.Text = "";
                flag = false;
            }

            if (!ExtraModules.isEmail(ToEmail.Text))
            {
                ToEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ToEmail.PlaceholderText = "Invalid Email";
                ToEmail.Text = "";
                flag = false;
            }

            return flag;
        }

        private void clearEmailInfoFields()
        {
            FromEmail.Text = "";
            ToEmail.Text = "";
            FromPassword.Password = "";

            FromEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
            ToEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

        }

        private void checkSelectedDates()
        {
            string FROM = AnalysisFromDayComboBox.SelectedItem.ToString() + "-" + (AnalysisFromMonthComboBox.SelectedIndex + 1).ToString() + "-" + AnalysisFromYearComboBox.SelectedItem.ToString();
            string pattern = "dd-M-yyyy";
            DateTime from;

            if (!DateTime.TryParseExact(FROM, pattern, null, System.Globalization.DateTimeStyles.None, out from))
            {
                AnalysisFromDayComboBox.SelectedIndex = 0;
                AnalysisFromMonthComboBox.SelectedIndex = 0;
                AnalysisFromYearComboBox.SelectedIndex = 0;
            }

            string TO = AnalysisToDayComboBox.SelectedItem.ToString() + "-" + (AnalysisToMonthComboBox.SelectedIndex + 1).ToString() + "-" + AnalysisToYearComboBox.SelectedItem.ToString();
            DateTime to;

            if (!DateTime.TryParseExact(TO, pattern, null, System.Globalization.DateTimeStyles.None, out to))
            {
                AnalysisToDayComboBox.SelectedIndex = DateTime.Now.Day - 1;
                AnalysisToMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
                AnalysisToYearComboBox.SelectedItem = DateTime.Now.Year;
            }

        }
    }
}
