using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Turbo.Control.UWP.Views
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
            txtOneDriveClientId.Text = settings.Values[nameof(DeviceSettingNames.OneDriveClientId)] as string ?? string.Empty;
            txtOneDriveClientSecret.Text = settings.Values[nameof(DeviceSettingNames.OneDriveClientSecret)] as string ?? string.Empty;

            txtDeviceHost.Text = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
            txtDevicePort.Text = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;
            
        }

        private void SaveSettings()
        {
            settings.Values[nameof(DeviceSettingNames.OneDriveClientId)] = txtOneDriveClientId.Text;
            settings.Values[nameof(DeviceSettingNames.OneDriveClientSecret)] = txtOneDriveClientSecret.Text;

            settings.Values[nameof(DeviceSettingNames.DeviceHost)] = txtDeviceHost.Text;
            settings.Values[nameof(DeviceSettingNames.DevicePort)] = txtDevicePort.Text;

        }
    }
}
