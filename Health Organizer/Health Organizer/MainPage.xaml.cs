using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI;
using SQLiteWinRT;

namespace Health_Organizer
{
    public sealed partial class MainPage : Page
    {
        private Database database;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeDB();
            HomeScreenImageAnimation.Begin();
            database = App.database;
        }

        private async void InitializeDB()
        {
            await App.InitializeDB();
        }

        private async void SignInClicked(object sender, RoutedEventArgs e)
        {
            if (checkSignInFields())
            {
                this.SignIn();
            }
        }

        private void SignUpClicked(object sender, RoutedEventArgs e)
        {
            MainPageCustomDialog.IsOpen = true;
        }

        private async void SignInEnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                if (checkSignInFields())
                {
                    this.SignIn();
                }
            }
        }

        private async void SignIn()
        {
            string error = "";
            try{
                MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MainPageProgressRing.IsActive = true;
                bool temp = await checkLogin();
                MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MainPageProgressRing.IsActive = false;
                if (temp == true)
                {
                    if (this.Frame != null)
                    {
                        this.Frame.Navigate(typeof(MainMenuPage));
                    }
                }
                else
                {
                    MainPagePassword.Password = "";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in MainPage---SignIn");
                Debug.WriteLine(ex.Message.ToString());
                error = ex.Message;
                MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MainPageProgressRing.IsActive = false;
                MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            //below code is to be commented when server is working
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainMenuPage));
            }

            if (!error.Equals(""))
            {
                var messageDialog = new MessageDialog(error, "");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK", null));
                var dialogResult = await messageDialog.ShowAsync();
            }
            
        }


        private async Task<bool> checkLogin()
        {
            if (ExtraModules.IsInternet())
            {
                var data = new List<KeyValuePair<string, string>>
                     {
                         new KeyValuePair<string, string>("username", MainPageUsername.Text.ToString()),
                         new KeyValuePair<string, string>("password", MainPagePassword.Password.ToString())
                      };
                string output = await LoginServer(data);
                if (output.Equals("success"))
                {
                    return true;
                }
                else
                {
                    var messageDialog = new MessageDialog(output, "Error");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK", null));
                    var dialogResult = await messageDialog.ShowAsync();
                }
            }
            return false;  //this shud be uncommented for running with server
            //return true;
        }

        private async Task<string> LoginServer(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://localhost:63342/Ic2014/login.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("MainPage" + responseString);
                JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                string error = root.GetNamedString("error");
                if (error.Equals("Success"))
                {
                    string username = root.GetNamedString("Username");
                    string userid = root.GetNamedString("UserId").ToString();
                    return "success";
                }
                else
                {
                    return error;
                }

            }
            return "Check internet Connection";
        }

        private async void SignUpClickedCallisto(object sender, RoutedEventArgs e)
        {
            string error = "";
            this.setAllSignUpFieldsWhite();
            try
            {
                if (checkSignUpFields())
                {
                    MainPageCustomDialog.IsOpen = false;

                    MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MainPageProgressRing.IsActive = true;
                    await SignUpNewUser();
                    MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    MainPageProgressRing.IsActive = false;
                    MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in MainPage---SignUpClickedCallisto");
                Debug.WriteLine(ex.Message.ToString());
                error = ex.Message;
                MainPageProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MainPageProgressRing.IsActive = false;
                MainPageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MainPageProgressRingTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!error.Equals(""))
            {
                var messageDialog = new MessageDialog(error, "");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK", null));
                var dialogResult = await messageDialog.ShowAsync();
            }

        }

        private void CancleSignUpClicked(object sender, RoutedEventArgs e)
        {
            MainPageCustomDialog.IsOpen = false;
            this.setAllSignUpFieldsWhite();
            MainPageSignUpNGOName.Text = "";
            MainPageSignUpEmail.Text = "";
            MainPageSignUpUsername.Text = "";
            MainPageSignUpPassword.Password = "";
        }

        private async Task SignUpNewUser()
        {
            if (ExtraModules.IsInternet())
            {
                var data = new List<KeyValuePair<string, string>>
                     {
                         new KeyValuePair<string, string>("username", MainPageSignUpUsername.Text),
                         new KeyValuePair<string, string>("password", MainPageSignUpPassword.Password.ToString()),
                         new KeyValuePair<string, string>("organisation",MainPageSignUpNGOName.Text),
                         new KeyValuePair<string, string>("email",MainPageSignUpEmail.Text)
                      };
                string output = await SignupServer(data);

                var messageDialog = new MessageDialog(output, "");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK", null));
                var dialogResult = await messageDialog.ShowAsync();
            }
            return;
        }

        private async Task<string> SignupServer(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://localhost:63342/Ic2014/signup.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Signup Output" + responseString);
                JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                string error = root.GetNamedString("error");
                return error;
            }
            return "Check internet Connection";
        }

        private void PointerEnteredEvent(object sender, PointerRoutedEventArgs e)
        {
            TextBlock block = sender as TextBlock;
            block.Foreground = new SolidColorBrush(Colors.SkyBlue);
        }

        private void PointerExitedEvent(object sender, PointerRoutedEventArgs e)
        {
            TextBlock block = sender as TextBlock;
            block.Foreground = new SolidColorBrush(Colors.White);
        }

        private void setAllSignUpFieldsWhite()
        {
            MainPageSignUpNGOName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
            MainPageSignUpEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
            MainPageSignUpUsername.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
            MainPageSignUpPassword.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
        }

        private void setAllSignInFieldsWhite()
        {
            MainPageUsername.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
            MainPagePassword.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
        }

        private bool checkSignUpFields()
        {
            if (MainPageSignUpNGOName.Text.Equals(""))
            {
                MainPageSignUpNGOName.Focus(FocusState.Keyboard);
                MainPageSignUpNGOName.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            if (MainPageSignUpEmail.Text.Equals(""))
            {
                MainPageSignUpEmail.Focus(FocusState.Keyboard);
                MainPageSignUpEmail.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            if (MainPageSignUpUsername.Text.Equals(""))
            {
                MainPageSignUpUsername.Focus(FocusState.Keyboard);
                MainPageSignUpUsername.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            if (MainPageSignUpPassword.Password.ToString() == "")
            {
                MainPageSignUpPassword.Focus(FocusState.Keyboard);
                MainPageSignUpPassword.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            return true;
        }

        private bool checkSignInFields()
        {
            this.setAllSignInFieldsWhite();
            if (MainPageUsername.Text.Trim().Equals(""))
            {
                MainPageUsername.Focus(FocusState.Keyboard);
                MainPageUsername.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            if (MainPagePassword.Password.Trim().Equals(""))
            {
                MainPagePassword.Focus(FocusState.Keyboard);
                MainPagePassword.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }

            return true;
        }
    }
}
