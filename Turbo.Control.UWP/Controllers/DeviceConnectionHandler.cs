using System;
using System.Threading.Tasks;
using Devices.Controllers.Base;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Turbo.Control.UWP.Controllers
{
    public class DeviceConnectionHandler
    {
        private static DeviceConnectionHandler instance = new DeviceConnectionHandler();
        ApplicationDataContainer settings;

        private DeviceConnectionHandler()
        {
            settings = ApplicationData.Current.LocalSettings;
        }

        public static DeviceConnectionHandler Instance { get { return instance; } }

        public bool Connected { get { return ControllerHandler.Connected; } 
        }

        public static string ConnectionFlyoutText
        {
            get
            {
                ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
                string deviceHost = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
                string devicePort = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;
                return $"Please wait while connecting to device \"{deviceHost}\" on port \"{devicePort}\".";
            }
        }

        public bool CheckParametersSet()
        {
            return (!string.IsNullOrWhiteSpace(settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string)
                && !string.IsNullOrWhiteSpace(settings.Values[nameof(DeviceSettingNames.DevicePort)] as string));
        }

        public async Task<bool> Connect()
        {
            string deviceHost = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
            string devicePort = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;

            return await ControllerHandler.InitializeConnection(deviceHost, devicePort);
        }

        public async Task Disconnect()
        {
            await ControllerHandler.Disconnect();
        }

        public async Task<ContentDialogResult> ShowConnectionFailedDialog()
        {
            string deviceHost = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
            string devicePort = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Connection failed",
                Content = $"Connection to \"{deviceHost}\" on port \"{devicePort}\" failed or could not be established. Please check the device is active and the connection parameters are correct.\r\n\r\nClick Retry to try again, or Settings to open the Application Settings.",
                PrimaryButtonText = "Retry",
                SecondaryButtonText = "Settings"
            };
            return await dialog.ShowAsync();
        }

        public async Task<ContentDialogResult> ShowMissingHostParametersDialog()
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Device HostName/IP or Port missing",
                Content = "Please specify the device host name or IP address and Port.\r\n\r\nWhen you click OK you will be redirected to the Application Settings page.",
                PrimaryButtonText = "Ok"
            };
            return await dialog.ShowAsync();
        }


    }
}
