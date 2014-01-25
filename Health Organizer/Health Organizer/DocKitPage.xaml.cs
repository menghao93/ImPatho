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
using System.Diagnostics;
using Health_Organizer.Database_Connet_Classes;
using Health_Organizer.DML_Method_Classes;
using System.Threading.Tasks;
using SQLite;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using Health_Organizer.Data_Model_Classes;

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
        private ObservableCollection<string> ocString, ocSearchList;
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
            this.ocString = new ObservableCollection<string>();
            //this.ocString.CollectionChanged += ocString_CollectionChanged;
            docKitListBox.ItemsSource = this.ocString;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (await this.InitializeDB())
            {
                docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                docKitProgress.IsActive = false;
            }
            docKitCombo.SelectedIndex = 0;
        }

        private async Task<bool> InitializeDB()
        {
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;
            connect = new DBConnect();
            await connect.InitializeDatabase(DBConnect.DOC_KIT_DB);
            diseaseMethods = new DiseasesTable(connect);
            firstAidMethods = new FirstAidTable(connect);
            return true;
        }

        private void docKitSearchClick(object sender, RoutedEventArgs e)
        {
            this.searchBoxAnimation();
        }

        private void docKitComboBox(object sender, SelectionChangedEventArgs e)
        {
            this.ocString.Clear();
            if (isSearching == true)
            {
                searchBoxOutAnimation.Stop();
                this.isSearching = false;
                searchBoxOutAnimation.Begin();
            }
            if (docKitListBox.ItemsSource != this.ocString)
                docKitListBox.ItemsSource = this.ocString;

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
                docKitCustomDialogAnimation.Begin();
            }
            else
            {
                docKitCustomDialogAnimationFA.Begin();
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
                    BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocString[docKitListBox.SelectedIndex].ToString());
                    docKitDName.Text = tempDisease.Name;
                    docKitDDescription.Text = tempDisease.Description;
                    docKitDSymptoms.Text = tempDisease.Symptoms;
                    docKitDImage.Text = tempDisease.Name + ".jpg";
                    decodedImage = tempDisease.Image;
                    isUpdating = true;
                    docKitDName.IsReadOnly = true;
                    docKitCustomDialogAnimation.Begin();
                }
                else
                {
                    docKitDialogFirstAid.IsOpen = true;
                    BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocString[docKitListBox.SelectedIndex].ToString());
                    docKitFAName.Text = tempFirstAid.Name;
                    docKitFADescription.Text = tempFirstAid.FirstAid;
                    docKitFASymptoms.Text = tempFirstAid.DoNot;
                    docKitFAImage.Text = tempFirstAid.Name + ".jpg";
                    decodedImage = tempFirstAid.Image;
                    isUpdating = true;
                    docKitFAName.IsReadOnly = true;
                    docKitCustomDialogAnimationFA.Begin();
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
                        BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocString[docKitListBox.SelectedIndex].ToString());
                        await diseaseMethods.DeleteDisease(tempDisease);
                    }
                    else
                    {
                        BasicFirstAid tempDisease = await firstAidMethods.FindSingleFirstAid(this.ocString[docKitListBox.SelectedIndex].ToString());
                        await firstAidMethods.DeleteFirstAid(tempDisease);
                    }
                    this.ocString.RemoveAt(docKitListBox.SelectedIndex);
                    if (this.ocString.Count() > 0)
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
            List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();

            //This is used to sort the list on the basis of Name value pairs. Also note first we need to clear previous list.
            result.Sort(delegate(BasicDiseases c1, BasicDiseases c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });

            foreach (var i in result)
            {
                this.ocString.Add(i.Name);
            }

            //Disable Edit/Delete Buttons if there are no items in the List.
            if (this.ocString.Count() > 0)
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


        }

        public async void UpdateFirstAidListBox()
        {

            List<BasicFirstAid> result = await firstAidMethods.SelectAllFirstAids();

            result.Sort(delegate(BasicFirstAid c1, BasicFirstAid c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });

            foreach (var i in result)
            {
                this.ocString.Add(i.Name);
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


        }

        private void showDiseaseItems()
        {
            docKitSymptomsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            docKitSymptomsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
        private async Task<bool> UpdateDiseaseData(BasicDiseases tempDisease)
        {
            docKitName.Text = tempDisease.Name;
            docKitDescription.Text = "\n" + tempDisease.Description;
            docKitImage.Source = await ImageMethods.Base64StringToBitmap(tempDisease.Image);

            docKitSymptomsPanel.Children.Clear();
            foreach (var i in tempDisease.Symptoms.Split(','))
            {
                if (i.Equals(""))
                {
                    continue;
                }

                StackPanel docKitSymptomsStackPanels = new StackPanel();
                docKitSymptomsStackPanels.Margin = new Thickness(0, 15, 0, 0);
                docKitSymptomsStackPanels.Orientation = Orientation.Horizontal;

                TextBlock dot = new TextBlock();
                dot.Width = 15;
                dot.FontSize = 20;
                dot.Text = "•";
                docKitSymptomsStackPanels.Children.Add(dot);

                TextBlock Symptom = new TextBlock();
                Symptom.Width = 650;
                Symptom.Text = ExtraModules.RemoveStringSpace(i).Replace(",","");
                Symptom.TextWrapping = TextWrapping.Wrap;
                Symptom.FontSize = 20;
                docKitSymptomsStackPanels.Children.Add(Symptom);

                docKitSymptomsPanel.Children.Add(docKitSymptomsStackPanels);
            }
            return true;
        }

        private async Task<bool> UpdateFirstAidData(BasicFirstAid tempFirstAid)
        {
            docKitName.Text = tempFirstAid.Name;
            docKitFirstAidDescription.Text = "\n" + tempFirstAid.FirstAid;
            docKitFirstAidImage.Source = await ImageMethods.Base64StringToBitmap(tempFirstAid.Image);
            docKitFirstAidSymptoms.Text = "\n" + tempFirstAid.DoNot;
            return true;
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
                                BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(this.ocString[docKitListBox.SelectedIndex].ToString());

                                tempDisease.Name = docKitDName.Text;
                                tempDisease.Description = docKitDDescription.Text;
                                tempDisease.Image = decodedImage;
                                tempDisease.Symptoms = docKitDSymptoms.Text;



                                docKitDSymptoms.Text = ExtraModules.RemoveExtraCommas(docKitDSymptoms.Text);
                                tempDisease.Symptoms = docKitDSymptoms.Text;

                                await diseaseMethods.UpdateDisease(tempDisease);
                                isUpdating = false;
                                docKitDName.IsReadOnly = false;
                                await this.UpdateDiseaseData(tempDisease);
                            }
                        }
                        else
                        {
                            await diseaseMethods.InsertDisease(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = ExtraModules.RemoveExtraCommas(docKitDSymptoms.Text), Image = decodedImage });
                            this.ocString.Add(docKitDName.Text);
                            docKitListBox.SelectedIndex = this.ocString.IndexOf(docKitDName.Text);
                            if (ocString.Count() == 1)
                            {
                                this.showDiseaseItems();
                                await this.UpdateDiseaseData(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = ExtraModules.RemoveExtraCommas(docKitDSymptoms.Text), Image = decodedImage });
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
                            BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocString[docKitListBox.SelectedIndex].ToString());
                            tempFirstAid.Name = docKitFAName.Text;
                            tempFirstAid.FirstAid = docKitFADescription.Text;
                            tempFirstAid.Image = decodedImage;
                            tempFirstAid.DoNot = docKitFASymptoms.Text;

                            await firstAidMethods.UpdateFirstAid(tempFirstAid);
                            isUpdating = false;
                            docKitFAName.IsReadOnly = false;
                            await this.UpdateFirstAidData(tempFirstAid);
                        }
                    }
                    else
                    {

                        await firstAidMethods.InsertFirstAid(new BasicFirstAid() { Name = docKitFAName.Text, FirstAid = docKitFADescription.Text, DoNot = docKitFASymptoms.Text, Image = decodedImage });
                        this.ocString.Add(docKitFAName.Text);
                        docKitListBox.SelectedIndex = this.ocString.IndexOf(docKitFAName.Text);
                        if (ocString.Count() == 1)
                        {
                            this.showFirstAidItems();
                            await this.UpdateFirstAidData(new BasicFirstAid() { Name = docKitFAName.Text, FirstAid = docKitFADescription.Text, DoNot = docKitFASymptoms.Text, Image = decodedImage });
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
                        tempDisease = await diseaseMethods.FindSingleDisease(this.ocString[docKitListBox.SelectedIndex].ToString());

                    }
                    await this.UpdateDiseaseData(tempDisease);
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
                        tempFirstAid = await firstAidMethods.FindSingleFirstAid(this.ocString[docKitListBox.SelectedIndex].ToString());

                    }
                    docKitScrollerFirstAid.ChangeView(0, 0, 1);
                    await this.UpdateFirstAidData(tempFirstAid);
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
            if (this.ocString.Count() < 0)
                return;

            this.searchList = this.ocString.Where(x => x.ToLower().Contains(args.QueryText.ToLower())).ToList();
            if (ocSearchList.Count() > 0)
                this.ocSearchList.Clear();
            foreach (string i in searchList)
            {
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
                countEnter++;
            }
            if (countEnter > 1 && (uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                t.AcceptsReturn = false;
            }


        }

        private void keyDown_newline(object sender, KeyRoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if ((uint)e.Key != (uint)Windows.System.VirtualKey.Enter && (uint)e.Key != (uint)Windows.System.VirtualKey.Space && (uint)e.Key != (uint)Windows.System.VirtualKey.Back)
            {
                t.AcceptsReturn = true;
                countEnter = 0;
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
                this.searchList = this.ocString.ToList();
                this.ocSearchList = new ObservableCollection<string>(this.searchList);
                docKitListBox.ItemsSource = this.ocSearchList;
            }
            else
            {
                Canvas.SetLeft(docKitSearchBox, 30);
                searchBoxInAnimation.Stop();
                searchBoxOutAnimation.Begin();
                docKitListBox.ItemsSource = this.ocString;
            }
        }
        //For sorting the diseases
        private void SortAscending(object sender, RoutedEventArgs e)
        {
            searchList = ocString.ToList();
            ocString.Clear();
            searchList.Sort(delegate(String c1, String c2)
            {
                return c1.CompareTo(c2);
            });
            this.ocString = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.docKitListBox.ItemsSource = this.ocString;
        }

        private void SortDescending(object sender, RoutedEventArgs e)
        {
            searchList = ocString.ToList();
            ocString.Clear();
            searchList.Sort(delegate(String c1, String c2)
            {
                return c2.CompareTo(c1);
            });
            this.ocString = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.docKitListBox.ItemsSource = this.ocString;
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
