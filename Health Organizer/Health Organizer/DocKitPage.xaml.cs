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
using System.Diagnostics;
using Health_Organizer.Data_Model_Classes;
using Health_Organizer.Database_Connet_Classes;
using Health_Organizer.DML_Method_Classes;
using System.Threading.Tasks;
using SQLite;
using Windows.UI.Popups;
using System.Collections.ObjectModel;

namespace Health_Organizer
{
    public sealed partial class DocKitPage : Page
    {
        DiseasesTable diseaseMethods;
        FirstAidTable firstAidMethods;
        DBConnect connect;
        private bool isUpdating = false, isDiseaseSelected = true, isSearching = false;
        private string decodedImage = null;
        private NavigationHelper navigationHelper;
        private ObservableCollection<string> ocStrings, ocSearchList;
        private List<string> searchList;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        int countEnter = 0;
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public DocKitPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);

            //Registering the Methods to be triggered for state change
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            //Registering the Methods to be triggered for state change in Collections of ListBox
            this.ocStrings = new ObservableCollection<string>();
            //this.ocStrings.CollectionChanged += ocStrings_CollectionChanged;
            docKitListBox.ItemsSource = this.ocStrings;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await this.InitializeDB();
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitProgress.IsActive = false;
            docKitCombo.SelectedIndex = 0;
        }

        private async Task InitializeDB()
        {
            connect = new DBConnect();
            await connect.InitializeDatabase(DBConnect.DOC_KIT_DB);
            diseaseMethods = new DiseasesTable(connect);
            firstAidMethods = new FirstAidTable(connect);
        }

        private void docKitSearchClick(object sender, RoutedEventArgs e)
        {
            this.searchBoxAnimation();
        }

        private void docKitComboBox(object sender, SelectionChangedEventArgs e)
        {
            this.ocStrings.Clear();
            if (isSearching == true)
            {
                searchBoxOutAnimation.Stop();
                this.isSearching = false;
                searchBoxOutAnimation.Begin();
            }
            if (docKitListBox.ItemsSource != this.ocStrings)
                docKitListBox.ItemsSource = this.ocStrings;

            if (docKitCombo.SelectedIndex == 0)
            {
                pageTitle.Text = "Disease List";
                docKitScrollerFirstAid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                docKitScrollerDisease.Visibility = Windows.UI.Xaml.Visibility.Visible;
                isDiseaseSelected = true;
                this.UpdateDiseaseListBox();
                docKitAddBut.Label = "Add Disease";
                docKitEditBut.Label = "Edit Disease";
                docKitDelBut.Label = "Remove Disease";
            }
            else if (docKitCombo.SelectedIndex == 1)
            {
                pageTitle.Text = "First Aid List";
                docKitScrollerDisease.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                docKitScrollerFirstAid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                isDiseaseSelected = false;
                this.UpdateFirstAidListBox();
                docKitAddBut.Label = "Add Aid";
                docKitEditBut.Label = "Edit Aid";
                docKitDelBut.Label = "Remove Aid";
            }
        }

        /////////////////////This methods are for Buttons in CommandBar at the bottom
        private void docKitAddItem(object sender, RoutedEventArgs e)
        {
            if (isDiseaseSelected)
            {
                docKitDialog.IsOpen = true;
            }
            else
            {
                docKitDialogFirstAid.IsOpen = true;
            }
            docKitCmdbar.IsOpen = false;
        }

        private async void docKitEditItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;
            countEnter = 0;
            if (docKitListBox.SelectedItem != null)
            {
                //Load all the values from the DB. Assertion: Value exist in DB since loaded from list.Also set PK to ReadOnly.
                if (isDiseaseSelected)
                {
                    docKitDialog.IsOpen = true;
                    BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocStrings[docKitListBox.SelectedIndex].ToString());
                    docKitDName.Text = tempDisease.Name;
                    docKitDDescription.Text = tempDisease.Description;
                    docKitDSymptoms.Text = tempDisease.Symptoms;
                    docKitDImage.Text = tempDisease.Name + ".jpg";
                    decodedImage = tempDisease.Image;
                    isUpdating = true;
                    docKitDName.IsReadOnly = true;
                }
                else
                {
                    docKitDialogFirstAid.IsOpen = true;
                    BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocStrings[docKitListBox.SelectedIndex].ToString());
                    docKitFAName.Text = tempFirstAid.Name;
                    docKitFADescription.Text = tempFirstAid.FirstAid;
                    docKitFASymptoms.Text = tempFirstAid.DoNot;
                    docKitFAImage.Text = tempFirstAid.Name + ".jpg";
                    decodedImage = tempFirstAid.Image;
                    isUpdating = true;
                    docKitFAName.IsReadOnly = true;
                }
            }
        }

        private async void docKitDelItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;

            //Find the instance of the item which is selected from the DB, then delete it using that instance.
            if (docKitListBox.SelectedItem != null)
            {
                var messageDialog = new MessageDialog("Are you sure you want to delete details for this disease?", "Confirmation");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes", null));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("No", null));
                var dialogResult = await messageDialog.ShowAsync();

                if (dialogResult.Label.Equals("Yes"))
                {
                    if (isDiseaseSelected)
                    {
                        BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocStrings[docKitListBox.SelectedIndex].ToString());
                        await diseaseMethods.DeleteDisease(tempDisease);
                    }
                    else
                    {
                        BasicFirstAid tempDisease = await firstAidMethods.FindSingleFirstAid(this.ocStrings[docKitListBox.SelectedIndex].ToString());
                        await firstAidMethods.DeleteFirstAid(tempDisease);
                    }
                    this.ocStrings.RemoveAt(docKitListBox.SelectedIndex);
                    if (this.ocStrings.Count() > 0)
                    {
                        docKitListBox.SelectedIndex = 0;
                    }
                    else
                    {
                        docKitDelBut.IsEnabled = false;
                        docKitEditBut.IsEnabled = false;
                    }
                }
            }
        }

        ////////////////////////This methods are for Updating the View after changes in FB
        private async void UpdateDiseaseListBox()
        {
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;

            List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();

            //This is used to sort the list on the basis of Name value pairs. Also note first we need to clear previous list.
            result.Sort(delegate(BasicDiseases c1, BasicDiseases c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });

            foreach (var i in result)
            {
                this.ocStrings.Add(i.Name);
            }

            //Disable Edit/Delete Buttons if there are no items in the List.
            if (this.ocStrings.Count() > 0)
            {
                if (docKitListBox.SelectedItem == null)
                    docKitListBox.SelectedIndex = 0;

                this.showDiseaseItems();
            }
            else
            {
                this.hideDiseaseItems();
                docKitDelBut.IsEnabled = false;
                docKitEditBut.IsEnabled = false;
            }

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = false;
        }

        public async void UpdateFirstAidListBox()
        {
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;

            List<BasicFirstAid> result = await firstAidMethods.SelectAllFirstAids();

            result.Sort(delegate(BasicFirstAid c1, BasicFirstAid c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });

            foreach (var i in result)
            {
                this.ocStrings.Add(i.Name);
            }

            if (result.Count() > 0)
            {
                if (docKitListBox.SelectedItem == null)
                    docKitListBox.SelectedIndex = 0;
                this.showFirstAidItems();
            }
            else
            {
                this.hideFirstAidItems();
                docKitDelBut.IsEnabled = false;
                docKitEditBut.IsEnabled = false;
            }

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = false;
        }

        private void showDiseaseItems()
        {
            docKitSymptoms.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitD.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitS.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void showFirstAidItems()
        {
            docKitFirstAidSymptoms.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidD.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidS.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void hideDiseaseItems()
        {
            docKitSymptoms.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitD.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitS.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void hideFirstAidItems()
        {
            docKitFirstAidSymptoms.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidD.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidS.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        ///////////////////This are the methods used to update the display immideately after updating
        private async void UpdateDiseaseData(BasicDiseases tempDisease)
        {
            docKitName.Text = tempDisease.Name;
            docKitDescription.Text = "\n" + tempDisease.Description;
            docKitImage.Source = await ImageMethods.Base64StringToBitmap(tempDisease.Image);

            string tempSymptoms = "";
            foreach (var i in tempDisease.Symptoms.Split(','))
            {
                tempSymptoms += "\n• " + i;
            }
            docKitSymptoms.Text = tempSymptoms;
        }

        private async void UpdateFirstAidData(BasicFirstAid tempFirstAid)
        {
            docKitName.Text = tempFirstAid.Name;
            docKitFirstAidDescription.Text = "\n" + tempFirstAid.FirstAid;
            docKitFirstAidImage.Source = await ImageMethods.Base64StringToBitmap(tempFirstAid.Image);
            docKitFirstAidSymptoms.Text = "\n" + tempFirstAid.DoNot;
        }

        //////////////////////This methods are for Buttons click events in Dialog Box opened.
        private async void docKitDialogSave(object sender, RoutedEventArgs e)
        {
            if (isDiseaseSelected)
            {
                if (docKitDName.Text.Equals("") || docKitDSymptoms.Text.Equals("") || docKitDDescription.Text.Equals("") || docKitDImage.Text.Equals(""))
                {
                    docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (decodedImage != null)
                    {
                        if (isUpdating == true)
                        {
                            //Find that object's instance and change its values
                            if (docKitListBox.SelectedItem != null)
                            {
                                BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocStrings[docKitListBox.SelectedIndex].ToString());

                                tempDisease.Name = docKitDName.Text;
                                tempDisease.Description = docKitDDescription.Text;
                                tempDisease.Image = decodedImage;
                                tempDisease.Symptoms = docKitDSymptoms.Text;
                                String temp = "";


                                foreach (var i in tempDisease.Symptoms.Split(','))
                                {

                                    if (i.Equals(""))
                                        continue;


                                    temp += i + ",";
                                }

                                docKitDSymptoms.Text = temp.Substring(0, temp.Length - 1);
                                tempDisease.Symptoms = docKitDSymptoms.Text;


                                await diseaseMethods.UpdateDisease(tempDisease);
                                isUpdating = false;
                                docKitDName.IsReadOnly = false;
                                this.UpdateDiseaseData(tempDisease);
                            }
                        }
                        else
                        {
                            await diseaseMethods.InsertDisease(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = docKitDSymptoms.Text, Image = decodedImage });
                            this.ocStrings.Add(docKitDName.Text);
                            docKitListBox.SelectedIndex = this.ocStrings.IndexOf(docKitDName.Text);
                            if (ocStrings.Count() == 1)
                            {
                                this.showDiseaseItems();
                                this.UpdateDiseaseData(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = docKitDSymptoms.Text, Image = decodedImage });
                                docKitDelBut.IsEnabled = true;
                                docKitEditBut.IsEnabled = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (docKitFAName.Text.Equals("") || docKitFASymptoms.Text.Equals("") || docKitFADescription.Text.Equals("") || docKitFAImage.Text.Equals(""))
                {
                    docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (isUpdating == true)
                    {
                        if (docKitListBox.SelectedItem != null)
                        {
                            //Find that object's instance and change its values
                            BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocStrings[docKitListBox.SelectedIndex].ToString());
                            tempFirstAid.Name = docKitFAName.Text;
                            tempFirstAid.FirstAid = docKitFADescription.Text;
                            tempFirstAid.Image = decodedImage;
                            tempFirstAid.DoNot = docKitFASymptoms.Text;

                            await firstAidMethods.UpdateFirstAid(tempFirstAid);
                            isUpdating = false;
                            docKitFAName.IsReadOnly = false;
                            this.UpdateFirstAidData(tempFirstAid);
                        }
                    }
                    else
                    {

                        await firstAidMethods.InsertFirstAid(new BasicFirstAid() { Name = docKitFAName.Text, FirstAid = docKitFADescription.Text, DoNot = docKitFASymptoms.Text, Image = decodedImage });
                        this.ocStrings.Add(docKitFAName.Text);
                        docKitListBox.SelectedIndex = this.ocStrings.IndexOf(docKitFAName.Text);
                        if (ocStrings.Count() == 1)
                        {
                            this.showFirstAidItems();
                            this.UpdateFirstAidData(new BasicFirstAid() { Name = docKitFAName.Text, FirstAid = docKitFADescription.Text, DoNot = docKitFASymptoms.Text, Image = decodedImage });
                            docKitDelBut.IsEnabled = true;
                            docKitEditBut.IsEnabled = true;
                        }
                    }
                }
            }
            //After everything is stored/Updated in database we need to reset all the fields.
            this.ClearFormFields();
        }

        private void docKitDialogCancel(object sender, RoutedEventArgs e)
        {
            if (isDiseaseSelected)
            {
                docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                docKitFAErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            this.ClearFormFields();
        }

        private async void docKitDialogBrowse(object sender, RoutedEventArgs e)
        {
            //This is used to Open the FilePicker Browse Menu from which we can select file
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            var file = await picker.PickSingleFileAsync();

            decodedImage = await ImageMethods.ConvertStorageFileToBase64String(file);
            if (isDiseaseSelected)
                docKitDImage.Text = file.Name;
            else
                docKitFAImage.Text = file.Name;
        }

        //////////////////////////This method is for the click event in the List Box.
        private async void docKitListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            TitleTextBlockAnimation.Stop();
            DiseaseGridAnimation.Stop();
            FirstAidGridAnimation.Stop();
            if (e.AddedItems.Count <= 0)
                return;

            if (docKitListBox.SelectedItem != null)
            {
                //Check whether diseases or firstaid and then display selected Item's details
                if (isDiseaseSelected)
                {
                    BasicDiseases tempDisease;
                    if (isSearching)
                    {
                        tempDisease = await diseaseMethods.FindSingleDisease(this.ocSearchList[docKitListBox.SelectedIndex].ToString());
                    }
                    else
                    {
                        tempDisease = await diseaseMethods.FindSingleDisease(this.ocStrings[docKitListBox.SelectedIndex].ToString());

                    }
                    this.UpdateDiseaseData(tempDisease);
                    TitleTextBlockAnimation.Begin();
                    DiseaseGridAnimation.Begin();
                    docKitScrollerDisease.ChangeView(0, 0, 1);
                }
                else
                {
                    BasicFirstAid tempFirstAid;
                    if (isSearching)
                    {
                        tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocSearchList[docKitListBox.SelectedIndex].ToString());
                    }
                    else
                    {
                        tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocStrings[docKitListBox.SelectedIndex].ToString());

                    }
                    docKitScrollerFirstAid.ChangeView(0, 0, 1);
                    this.UpdateFirstAidData(tempFirstAid);
                    TitleTextBlockAnimation.Begin();
                    FirstAidGridAnimation.Begin();
                }
            }
        }

        /////////////////////////This is used to clear all the Dialog Fields
        private void ClearFormFields()
        {
            if (docKitDialog.IsOpen == true)
                docKitDialog.IsOpen = false;
            docKitDName.Text = "";
            docKitDDescription.Text = "";
            docKitDSymptoms.Text = "";
            docKitDImage.Text = "";
            docKitDName.IsReadOnly = false;

            if (docKitDialogFirstAid.IsOpen == true)
                docKitDialogFirstAid.IsOpen = false;
            docKitFAName.Text = "";
            docKitFADescription.Text = "";
            docKitFASymptoms.Text = "";
            docKitFAImage.Text = "";
            decodedImage = null;
            docKitFAName.IsReadOnly = false;
        }

        ///////////////////////This module is used to filter the list box when we enter query in search box.
        private void docKitSearchBoxQueryChnaged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            if (this.ocStrings.Count() < 0)
                return;

            this.searchList = this.ocStrings.Where(x => x.ToLower().Contains(args.QueryText.ToLower())).ToList();
            if (ocSearchList.Count() > 0)
                this.ocSearchList.Clear();
            foreach (string i in searchList)
            {
                Debug.WriteLine(i);
                this.ocSearchList.Add(i);
            }
            isSearching = true;
        }

        /////////////////////This module is used hide elements after animations complete. Used to hiding search box.
        private void OutAnimationCompleted(object sender, object e)
        {
            docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void newLineCheck(object sender, KeyRoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;

            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                Debug.WriteLine(countEnter);
                countEnter++;
            }
            if (countEnter > 1 && (uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                Debug.WriteLine("avo");

                t.AcceptsReturn = false;
            }
           
            
        }

        private void keyDown_newline(object sender, KeyRoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if ((uint)e.Key != (uint)Windows.System.VirtualKey.Enter && (uint)e.Key != (uint)Windows.System.VirtualKey.Space && (uint)e.Key != (uint)Windows.System.VirtualKey.Back)
            { 
            t.AcceptsReturn = true;
            countEnter=0;
            }
        }

        private void KeyPressed(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (char)70)
            {
                this.searchBoxAnimation();
            }

            //Other if statements if you want to keep an eye out for other keyPresses
        }

        private void searchBoxAnimation()
        {
            if (docKitSearchBox.Visibility != Windows.UI.Xaml.Visibility.Visible)
            {
                searchBoxOutAnimation.Stop();
                Canvas.SetLeft(docKitSearchBox, 100);
                docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                searchBoxInAnimation.Begin();
                this.searchList = this.ocStrings.ToList();
                this.ocSearchList = new ObservableCollection<string>(this.searchList);
                docKitListBox.ItemsSource = this.ocSearchList;
            }
            else
            {
                Canvas.SetLeft(docKitSearchBox, 30);
                searchBoxInAnimation.Stop();
                searchBoxOutAnimation.Begin();
                docKitListBox.ItemsSource = this.ocStrings;
            }
        }
        //For sorting the diseases
        private void asc_option_Click(object sender, RoutedEventArgs e)
        {
            searchList = ocStrings.ToList();
            ocStrings.Clear();
            searchList.Sort(delegate(String c1, String c2)
            {
                return c1.CompareTo(c2);
            });
            this.ocStrings = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.docKitListBox.ItemsSource = this.ocStrings;
        }

        private void dsc_option_Click(object sender, RoutedEventArgs e)
        {
            searchList = ocStrings.ToList();
            ocStrings.Clear();
            searchList.Sort(delegate(String c1, String c2)
            {
                return c2.CompareTo(c1);
            });
            this.ocStrings = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.docKitListBox.ItemsSource = this.ocStrings;
        }
    }
}
