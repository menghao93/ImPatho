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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Health_Organizer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DocKitPage : Page
    {
        DiseasesTable diseaseMethods;
        DBConnect connect;
        private bool isUpdating = false;
        private string decodedImage = null;
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


        public DocKitPage()
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await this.InitializeDB();
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitProgress.IsActive = false;
            this.UpdateListBox();
        }

        private async Task InitializeDB()
        {
            connect = new DBConnect();
            await connect.InitializeDatabase();
            diseaseMethods = new DiseasesTable(connect);
        }

        private void docKitSearchBut(object sender, RoutedEventArgs e)
        {
            if (docKitSearchBox.Visibility != Windows.UI.Xaml.Visibility.Visible)
            {
                docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void docKitComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (docKitCombo.SelectedIndex == 0){
                pageTitle.Text = "Disease List";
            }
            else if (docKitCombo.SelectedIndex == 1)
            {
                pageTitle.Text = "First Aid List";
            }
        }

/////////////////////This methods are for Buttons in CommandBar at the bottom
        private void docKitAddItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;
            docKitDialog.IsOpen = true;
        }

        private async void docKitEditItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;

            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            if (xItem != null)
            {
                //Load all the values from the DB. Assertion: Value exist in DB since loaded from list.Also set PK to ReadOnly.
                docKitDialog.IsOpen = true;
                BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                docKitDName.Text = tempDisease.Name;
                docKitDDescription.Text = tempDisease.Description;
                docKitDSymptoms.Text = tempDisease.Symptoms;
                docKitDImage.Text = tempDisease.Name + ".jpg";
                decodedImage = tempDisease.Image;
                isUpdating = true;
                docKitDName.IsReadOnly = true;
            }
        }

        private async void docKitDelItem(object sender, RoutedEventArgs e)
        {
            //Find the instance of the item which is selected from the DB, then delete it using that instance.
            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            if (xItem != null)
            {
                BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                await diseaseMethods.DeleteDisease(tempDisease);

                this.UpdateListBox();
            }

            docKitCmdbar.IsOpen = false;
        }

////////////////////////This methods are for Updating the View after changes in FB
        private async void UpdateListBox()
        {

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;
            List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();

            //Disable Edit/Delete Buttons if there are no items in the List.
            if (result.Count() <= 0)
            {
                docKitDelBut.IsEnabled = false;
                docKitEditBut.IsEnabled = false;
            }
            else {
                docKitDelBut.IsEnabled = true;
                docKitEditBut.IsEnabled = true;
            }

            //This is used to sort the list on the basis of Name value pairs. Also note first we need to clear previous list.
            result.Sort(delegate(BasicDiseases c1, BasicDiseases c2) { 
                return c1.Name.CompareTo(c2.Name); 
            });
            docKitListBox.Items.Clear();

            //Load the Resource Style from themed dictionary for listboxItems
            ResourceDictionary rd =  Application.Current.Resources.ThemeDictionaries["Default"] as ResourceDictionary;
            
            foreach(var i in result)
            {
                //Load A New Item Programatically every time set its style to one required and then display it.
                ListBoxItem xItem = new ListBoxItem();
                xItem.Content = i.Name;
                xItem.Style = rd["ListBoxItemStyle"] as Style;
                docKitListBox.Items.Add(xItem);
            }

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = false;
        }

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

//////////////////////This methods are for Buttons click events in Dialog Box opened.
        private async void docKitDialogSave(object sender, RoutedEventArgs e)
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
                        ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
                        BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                        tempDisease.Name = docKitDName.Text;
                        tempDisease.Description = docKitDDescription.Text;
                        tempDisease.Image = decodedImage;
                        tempDisease.Symptoms = docKitDSymptoms.Text;

                        await diseaseMethods.UpdateDisease(tempDisease);
                        isUpdating = false;
                        docKitDName.IsReadOnly = false;
                        this.UpdateDiseaseData(tempDisease);
                    }
                    else
                    {

                        await diseaseMethods.InsertDisease(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = docKitDSymptoms.Text, Image = decodedImage });
                    }
                    //After everything is stored/Updated in database we need to reset all the fields.
                    docKitDialog.IsOpen = false;
                    docKitDName.Text = "";
                    docKitDDescription.Text = "";
                    docKitDSymptoms.Text = "";
                    docKitDImage.Text = "";
                    decodedImage = null;

                    this.UpdateListBox();
                }
            }
        }

        private void docKitDialogCancel(object sender, RoutedEventArgs e)
        {
            docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitDialog.IsOpen = false;
        }

        private async void docKitDialogBrowse(object sender, RoutedEventArgs e)
        {
            //This is used to Open the FilePicker Browse Menu from which we can select file
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            var file = await picker.PickSingleFileAsync();

            decodedImage = await ImageMethods.ConvertStorageFileToBase64String(file);
            //Debug.WriteLine(decodedImage);
            docKitDImage.Text = file.Name;
        }

//////////////////////This method is for the click event in the List Box.
        private async void docKitListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;
            
            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
            //Debug.WriteLine(tempDisease.Symptoms.Split(',').Count());
            this.UpdateDiseaseData(tempDisease);     
        }       
    }
}
