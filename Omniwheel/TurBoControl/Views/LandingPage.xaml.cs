using System;
using TurBoControl.Controller;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Common.Base;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TurBoControl.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {

        ApplicationDataContainer settings;
        ImageSourceController imageSource;

        public LandingPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;
            imageSource = new ImageSourceController();
            imageSource.OnImageReceived += ImageSource_OnImageReceived;
        }

        private void ImageSource_OnImageReceived(object sender, EventArgs e)
        {
            imageFrontCamera.Source = imageSource.CurrentImage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DeviceConnectionController.Instance.RegisterOnDataReceivedEvent("LandingPage", Instance_OnDataReceived);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DeviceConnectionController.Instance.UnRegisterOnDataReceivedEvent("LandingPage", Instance_OnDataReceived);
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
            else if (data.ContainsKey("Target") && data.GetNamedString("Target").ToUpperInvariant() == "FrontCamera".ToUpperInvariant() &&
                data.ContainsKey("Action") && data.GetNamedString("Action").ToUpperInvariant() == "Capture".ToUpperInvariant())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    byte[] buffer = Convert.FromBase64String(data.GetNamedString("ImageBase64"));
                    BitmapImage image = new BitmapImage();
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(buffer.AsBuffer());
                        stream.Seek(0);
                        await image.SetSourceAsync(stream);
                    }
                    imageFrontCamera.Source = image;
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

            if (!await DeviceConnectionController.Instance.Connect(deviceHost, devicePort))
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
            await imageSource.CaptureDeviceImage();
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
            if (DeviceConnectionController.Instance.ConnectionStatus == Common.Communication.ConnectionStatus.Connected)
            {
                JsonObject move = new JsonObject();
                move.AddValue("Target", "BrickPi.Drive");
                move.AddValue("Action", "Move");
                move.AddValue("Direction", e.Angle);
                move.AddValue("Velocity", e.Distance);
                move.AddValue("Rotation", 0);
                await DeviceConnectionController.Instance.Send("LandingPage", move);

            }
        }

        private async void Joypad_Released(object sender, Controls.JoypadEventArgs e)
        {
            if (DeviceConnectionController.Instance.ConnectionStatus == Common.Communication.ConnectionStatus.Connected)
            {
                JsonObject stop = new JsonObject();
                stop.AddValue("Target", "BrickPi.Drive");
                stop.AddValue("Action", "Stop");
                await DeviceConnectionController.Instance.Send("LandingPage", stop);

            }
        }

        private async void Joypad_Captured(object sender, EventArgs e)
        {
            if (DeviceConnectionController.Instance.ConnectionStatus == Common.Communication.ConnectionStatus.Connected)
            {
                JsonObject start = new JsonObject();
                start.AddValue("Target", "BrickPi.Drive");
                start.AddValue("Action", "Start");
                await DeviceConnectionController.Instance.Send("LandingPage", start);

            }

        }
    }
}
