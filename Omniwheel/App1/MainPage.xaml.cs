using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Communication;
using Common.Communication.Channels;
using Devices.Base;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    public class AppSettings
    {
        // Obtained from OneDrive Login
        public string OneDriveAccessToken = "";
        public string OneDriveRefreshToken = "";
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        OneDriveConnector connector;

        public MainPage()
        {
            this.InitializeComponent();
        }

        public const string clientId = "";
        public const string clientSecret = "";

        private AppSettings appSettings;

        private SocketClient socketClient;

        private async Task OneDrive()
        {
            if (null == connector || !connector.LoggedIn)
            {
                connector = new OneDriveConnector();

                connector.TokensChangedEvent += Connector_TokensChangedEvent;
                if (string.IsNullOrWhiteSpace(appSettings.OneDriveRefreshToken))
                    await connector.LoginAsync(clientId, clientSecret, accessToken);
                else
                    await connector.Reauthorize(clientId, clientSecret, appSettings.OneDriveRefreshToken);
            }
            if (connector.LoggedIn)
            {
                string folder = "";
                var files = await connector.ListFilesAsync(folder);
                //await connector.UploadFileAsync(file, "\\Pics");
            }
        }

        private string refreshToken = string.Empty;
        private string accessToken = string.Empty;


        private async void Connector_TokensChangedEvent(object sender, EventArgs e)
        {
            appSettings.OneDriveRefreshToken = (sender as OneDriveConnector).RefreshToken;
            appSettings.OneDriveAccessToken = (sender as OneDriveConnector).AccessToken;
            await SaveAsync(appSettings, fileName);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            this.appSettings = await RestoreAsync(fileName);

            await OneDrive();

        }

        public static readonly StorageFolder SettingsFolder = ApplicationData.Current.LocalFolder;
        string fileName = "Settings.xml";

        /// <summary>
        /// Save the settings to a file
        /// </summary>
        /// <param name="settings">Settings object to save</param>
        /// <param name="filename">Name of file to save to</param>
        /// <returns></returns>
        public static async Task SaveAsync(AppSettings settings, string filename)
        {
            StorageFile sessionFile = await SettingsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream sessionRandomAccess = await sessionFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings), new Type[] { typeof(AppSettings) });
                    serializer.Serialize(sessionOutputStream.AsStreamForWrite(), settings);
                    await sessionOutputStream.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Load the settings from a file
        /// </summary>
        /// <param name="filename">Name of settings file</param>
        /// <returns></returns>
        public static async Task<AppSettings> RestoreAsync(string filename)
        {
            try
            {
                StorageFile sessionFile = await SettingsFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                if (sessionFile == null)
                {
                    return new AppSettings();
                }
                using (IInputStream sessionInputStream = await sessionFile.OpenReadAsync())
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    return (AppSettings)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AppSettings.RestoreAsync(): " + ex.Message);

                // Log telemetry event about this exception
                var events = new Dictionary<string, string> { { "AppSettings", ex.Message } };

                // If settings.xml file is corrupted and cannot be read - behave as if it does not exist.
                return new AppSettings();
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (null != connector)
                await connector.LogoutAsync();
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            string folder = "/Pics";
            //string sourcePath = "C:\\Storage\\Misc\\DSCF9583.jpg";

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null && null != connector && connector.LoggedIn)
            {
                await connector.UploadFileAsync(file, folder);
            }

        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
//            string fileName = "C:\\Temp\\some.json";

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".json");
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".csv");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if (socketClient == null)
                    socketClient = new SocketClient();
                if (socketClient.ConnectionStatus != ConnectionStatus.Connected)
                {
                    ChannelBase channel = await socketClient.Connect("turbo", "8027", DataFormat.StringText);
                    channel.OnMessageReceived += SocketClient_OnMessageReceived;
                }
                using (var readStream = await file.OpenStreamForReadAsync())
                {
                    using (var streamReader = new StreamReader(readStream))
                        await socketClient.Send(Guid.Empty, streamReader.ReadToEnd() + Environment.NewLine);
                    //await SocketClient.Disconnect();
                }
                //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));
            }
        }

        private void SocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine((e as StringMessageArgs).Message);
        }

        private async void button4_Click(object sender, RoutedEventArgs e)
        {
            if (socketClient != null)
            {
                await socketClient.Disconnect();
                socketClient = null;
            }


        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            webView.Navigate(new Uri(OneDriveConnector.GenerateOneDriveLoginUrl(clientId)));
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            accessToken = OneDriveConnector.ParseAccessCode(webView.Source);
            textBox.Text = accessToken;
        }
    }
}