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
            DeviceConnection.Instance.RegisterOnDataReceivedEvent("LandingPage", Instance_OnDataReceived);
            JsonObject hello = new JsonObject();
            hello.AddValue("Target", "BrickPi.Led1");
            hello.AddValue("Action", "Status");
            await DeviceConnection.Instance.Send("LandingPage", hello);

                //await socketClient.Send(Guid.Empty, "ECHO");
                //await SocketClient.Disconnect();
            //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));

        }

        private async void Instance_OnDataReceived(JsonObject data)
        {
            if (data.ContainsKey("Target") && data.GetNamedString("Target").ToUpperInvariant() == "BrickPi.Led1".ToUpperInvariant() &&
                data.ContainsKey("Status"))
            {
                string status = data.GetNamedString("Status");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (status == "Enabled")
                    {
                        btnConnect.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
                    }
                    else
                        btnConnect.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
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

    }
}
