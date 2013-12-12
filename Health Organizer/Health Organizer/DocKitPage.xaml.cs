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
            //await diseaseMethods.InsertDisease(new BasicDiseases() {Name = "test 3", Description = "Fucking Awesome!", Symptoms = "dasfasfas"});
            //await diseaseMethods.InsertDisease(new BasicDiseases() { Name = "test 2", Description = "Fucking Awesome!2", Symptoms = "afdasds" });
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
            //List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();
            //foreach (var item in result)
            //{
            //    Debug.WriteLine(item.Name);   
            //}
        }

        private void docKitAddItem(object sender, RoutedEventArgs e)
        {
            docKitDialog.IsOpen = true;
        }

        private async void docKitDialogSave(object sender, RoutedEventArgs e)
        {                
            if (docKitDName.Text.Equals("") ||  docKitDSymptoms.Text.Equals("") || docKitDDescription.Text.Equals("") || docKitDImage.Text.Equals(""))
            {
                docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }else{
                if (decodedImage != null)
                {
                    try
                    {
                        await diseaseMethods.InsertDisease(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = docKitDSymptoms.Text, Image = decodedImage });

                        //After everything is stored in database we need to reset all the fields.
                        docKitDialog.IsOpen = false;
                        docKitDName.Text = "";
                        docKitDDescription.Text = "";
                        docKitDSymptoms.Text = "";
                        docKitDImage.Text = "";
                        decodedImage = null;
                        this.UpdateListBox();
                    }
                    catch (SQLite.SQLiteException ex)
                    {
                        docKitErrorDescription.Text = "Please Select unique name for diseases.";
                        docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
            }
        }

        private async void UpdateListBox()
        {

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;
            List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();
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
            Debug.WriteLine(decodedImage);
            docKitDImage.Text = file.Name;
        }
    }
}
