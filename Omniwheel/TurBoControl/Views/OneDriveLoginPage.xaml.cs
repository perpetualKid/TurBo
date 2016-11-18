using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Devices.Base;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TurBoControl.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OneDriveLoginPage : Page
    {
        ApplicationDataContainer settings;

        public OneDriveLoginPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string clientId = settings.Values["OneDriveClientId"] as string ?? string.Empty;
            Guid clientIdGuid; 
            if (string.IsNullOrWhiteSpace(clientId) || !Guid.TryParse(clientId, out clientIdGuid))
            {
                DisplayMissingClientIdDialog(e.Parameter);
                return;
            }

            webView.Navigate(new Uri(OneDriveConnector.GenerateOneDriveLoginUrl(clientId)));

            base.OnNavigatedTo(e);
        }

        private async void DisplayMissingClientIdDialog(object parameter)
        {
            ContentDialog missingClientId = new ContentDialog()
            {
                Title = "OneDrive ClientId missing",
                Content = "Please specify a valid OneDrive ClientId and ClientSecret.\r\n\r\nWhen you click OK you will be redirected to the Application Settings page.",
                PrimaryButtonText = "Ok"
            };

            ContentDialogResult result = await missingClientId.ShowAsync();
            this.Frame.Navigate(typeof(AppSettingsPage), parameter, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());

        }

        private void webView_LoadCompleted(object sender, NavigationEventArgs e)
        {
                string accessToken = OneDriveConnector.ParseAccessCode(webView.Source);
                txtAccessToken.Text = accessToken ?? string.Empty;
        }
    }
}
