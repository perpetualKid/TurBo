using System;
using System.Threading.Tasks;
using Devices.Communication;
using Devices.Controllers.Base;
using Devices.Util.Extensions;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Turbo.Control.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {

        ApplicationDataContainer settings;

        public LandingPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Connect(sender);
        }

        private async void Instance_OnDataReceived(JsonObject data)
        {
            if (data.ContainsKey("Target") && data.GetNamedString("Target").ToUpperInvariant() == "BrickPi.NxtColor.Port_S3".ToUpperInvariant() &&
                data.ContainsKey("Action") && data.GetNamedString("Action").ToUpperInvariant() == "ARGB")
            {
                //string status = data.GetNamedString("Status");
                byte red = (byte)data.GetNamedNumber("Red");
                byte green = (byte)data.GetNamedNumber("Green");
                byte blue = (byte)data.GetNamedNumber("Blue");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Color color = Color.FromArgb(0xFF, red, green, blue);
                    ellColor.Fill = new SolidColorBrush(color);
                    //if (status == "Enabled")
                    //{
                    //    btnConnect.Background = new SolidColorBrush(Colors.Blue);
                    //}
                    //else
                    //    btnConnect.Background = new SolidColorBrush(Colors.Gray);
                });

            }
        }

        private async Task Connect(object sender)
        {
            string deviceHost = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
            string devicePort = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;

            ConnectionFlyoutText.Text = $"Please wait while trying to connect to device \"{deviceHost}\" on port \"{devicePort}\".";
            ConnectionFlyoutText.MaxWidth = Window.Current.CoreWindow.Bounds.Width;
            ConnectionFlyout.ShowAt(btnConnect as FrameworkElement);

            if (string.IsNullOrWhiteSpace(deviceHost) || string.IsNullOrWhiteSpace(devicePort))
            {
                DisplayMissingHostParametersDialog(sender);
                return;
            }

            if (!await ControllerHandler.InitializeConnection(deviceHost, devicePort))
            {
                ConnectionFlyout.Hide();
                DisplayConnectionFailedDialog(sender, deviceHost, devicePort);
                return;
            }
            //JsonObject hello = new JsonObject();
            //hello.AddValue("Target", "BrickPi.NxtColor.Port_S3");
            //hello.AddValue("Action", "ARGB");
            //await DeviceConnection.Instance.Send("LandingPage", hello);
            //ellColor.Fill = new SolidColorBrush(Colors.White);
            //await socketClient.Send(Guid.Empty, "ECHO");
            //await SocketClient.Disconnect();
            //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));
            ConnectionFlyout.Hide();
            //await imageSource.CaptureDeviceImage();
        }

        private async void DisplayMissingHostParametersDialog(object parameter)
        {
            ContentDialog missingHostParameters = new ContentDialog()
            {
                Title = "Device HostName/IP or Port missing",
                Content = "Please specify the device host name or IP address and Port.\r\n\r\nWhen you click OK you will be redirected to the Application Settings page.",
                PrimaryButtonText = "Ok"
            };

            ContentDialogResult result = await missingHostParameters.ShowAsync();
            this.Frame.Navigate(typeof(AppSettingsPage), parameter, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void DisplayConnectionFailedDialog(object parameter, string deviceHost, string devicePort)
        {
            ContentDialog connectionFailed = new ContentDialog()
            {
                Title = "Connection failed",
                Content = $"Connection to \"{deviceHost}\" on port \"{devicePort}\" failed or could not be established. Please check the device is active and the connection parameters are correct.\r\n\r\nClick Retry to try again, or Settings to open the Application Settings.",
                PrimaryButtonText = "Retry",
                SecondaryButtonText = "Settings"
            };

            ContentDialogResult result = await connectionFailed.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    break;
                case ContentDialogResult.Primary:
                    await Connect(parameter);
                    break;
                case ContentDialogResult.Secondary:
                    this.Frame.Navigate(typeof(AppSettingsPage), parameter, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;
            }
        }


        private async void Joypad_Moved(object sender, Controls.JoypadEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance} Angle: {e.Angle}";
            if (ControllerHandler.Connection.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject move = new JsonObject();
                move.AddValue("Target", "BrickPi.Drive");
                move.AddValue("Action", "Move");
                move.AddValue("Direction", e.Angle);
                move.AddValue("Velocity", e.Distance);
                move.AddValue("Rotation", 0);
                await ControllerHandler.Connection.Send("LandingPage", move);

            }
        }

        private async void Joypad_Released(object sender, Controls.JoypadEventArgs e)
        {
            if (ControllerHandler.Connection.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject stop = new JsonObject();
                stop.AddValue("Target", "BrickPi.Drive");
                stop.AddValue("Action", "Stop");
                await ControllerHandler.Connection.Send("LandingPage", stop);

            }
        }

        private async void Joypad_Captured(object sender, EventArgs e)
        {
            if (ControllerHandler.Connection.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject start = new JsonObject();
                start.AddValue("Target", "BrickPi.Drive");
                start.AddValue("Action", "Start");
                await ControllerHandler.Connection.Send("LandingPage", start);

            }

        }
    }
}
