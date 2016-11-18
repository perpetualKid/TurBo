using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class AppSettingsPage : Page
    {
        ApplicationDataContainer settings;

        public AppSettingsPage()
        {
            this.InitializeComponent();

            settings = ApplicationData.Current.LocalSettings;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadSettings();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            SaveSettings();
            base.OnNavigatingFrom(e);
        }

        private void LoadSettings()
        {
            txtOneDriveClientId.Text = settings.Values["OneDriveClientId"] as string ?? string.Empty;
            txtOneDriveClientSecret.Text = settings.Values["OneDriveClientSecret"] as string ?? string.Empty;
        }

        private void SaveSettings()
        {
            settings.Values["OneDriveClientId"] = txtOneDriveClientId.Text;
            settings.Values["OneDriveClientSecret"] = txtOneDriveClientSecret.Text;
        }
    }
}
