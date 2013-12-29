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

namespace Health_Organizer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
            HomeScreenImageAnimation.Begin();
            //await this.InitializeDB();
        }

        private void sign_in_click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainMenuPage));
            }
        }

        private void sign_up_click(object sender, RoutedEventArgs e)
        {

        }

        private void SignInEnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Enter)
            {
                if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(MainMenuPage));
                }
            }
        }
    }
}
