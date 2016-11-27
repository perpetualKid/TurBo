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

namespace TurBoControl.Views
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
            DeviceConnection.Instance.RegisterOnDataReceivedEvent("LandingPage", Instance_OnDataReceived);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DeviceConnection.Instance.UnRegisterOnDataReceivedEvent("LandingPage", Instance_OnDataReceived);
            base.OnNavigatedFrom(e);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            string deviceHost = settings.Values[nameof(DeviceSettingNames.DeviceHost)] as string ?? string.Empty;
            string devicePort = settings.Values[nameof(DeviceSettingNames.DevicePort)] as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(deviceHost) || string.IsNullOrWhiteSpace(devicePort))
            {
                DisplayMissingHostParameters(e.OriginalSource);
                return;
            }

            await DeviceConnection.Instance.Connect(deviceHost, devicePort);
            JsonObject hello = new JsonObject();
            hello.AddValue("Target", "BrickPi.NxtColor.Port_S3");
            hello.AddValue("Action", "ARGB");
            await DeviceConnection.Instance.Send("LandingPage", hello);
            ellColor.Fill = new SolidColorBrush(Colors.White);
                //await socketClient.Send(Guid.Empty, "ECHO");
                //await SocketClient.Disconnect();
            //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));

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
            System.Diagnostics.Debug.WriteLine(data.Stringify());
        }

        private async void DisplayMissingHostParameters(object parameter)
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

        private void Joypad_Moved(object sender, Controls.JoypadEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance} Angle: {e.Angle}";
        }
    }
}
