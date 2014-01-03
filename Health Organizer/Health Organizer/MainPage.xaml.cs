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

namespace Health_Organizer
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeDB();
            HomeScreenImageAnimation.Begin();
        }

        private async void InitializeDB()
        {
            await App.InitializeDB();
        }

        private async void sign_in_click(object sender, RoutedEventArgs e)
        {
            bool temp = await checkLogin();
            if (temp == true)
            {
                if (this.Frame != null)
                {

                    this.Frame.Navigate(typeof(MainMenuPage));
                }

            }
        }

        private void sign_up_click(object sender, RoutedEventArgs e)
        {

        }

        private async void SignInEnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                bool temp=await checkLogin();
                if (temp==true) 
                {
                    if (this.Frame != null)
                    {

                        this.Frame.Navigate(typeof(MainMenuPage));
                    }
                    
                }
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
                string output=await LoginServer(data);
                if (output.Equals("success"))
                {
                    return true;
                }
            }
            return false;
            
        }

        private async Task<string> LoginServer(List<KeyValuePair<string, string>> values)
        {
            if (ExtraModules.IsInternet())
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://localhost:63342/Ic2014/login.php", new FormUrlEncodedContent(values));
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("MainPage"+responseString);
                JsonObject root = Windows.Data.Json.JsonValue.Parse(responseString).GetObject();
                string error = root.GetNamedString("error");
                if (error.Equals("Success"))
                {
                    string username = root.GetNamedString("Username");
                    string userid = root.GetNamedString("UserId").ToString();
                    return "success";
                }
                else {
                        return error;
                    }

            }
            return "Check internet Connection";
        }


    }
}
