using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Communication;
using Common.Communication.Channels;
using OmniWheel;
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

        public const string OneDriveRedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        public const string OneDriveLoginUrl = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=code&redirect_uri={2}";
        public const string OneDriveLogoutUrl = "https://login.live.com/oauth20_logout.srf?client_id={0}&redirect_uri={1}";
        public const string OneDriveScope = "wl.offline_access onedrive.readwrite";
        public const string OneDriveRootUrl = "https://api.onedrive.com/v1.0/drive/root:";
        public const string OneDriveTokenUrl = "https://login.live.com/oauth20_token.srf";
        public const string OneDriveTokenContent = "client_id={0}&redirect_uri={1}&client_secret={2}&{3}={4}&grant_type={5}";

        public const string clientId = "a80849fc-d08b-4e3c-910c-8ec92565774f";
        public const string clientSecret = "N4dogjq8kyCQVBLLrwrbrQg";
        public const string redirectUrl = "urn:ietf:wg:oauth:2.0:oob";

        private AppSettings appSettings;

        private SocketClient socketClient;

        private async Task OneDrive()
        {
            if (null == connector || !connector.isLoggedIn)
            {
                connector = new OmniWheel.OneDriveConnector();
                connector.TokensChangedEvent += Connector_TokensChangedEvent;
                if (string.IsNullOrWhiteSpace(appSettings.OneDriveRefreshToken))
                    await connector.LoginAsync(clientId, clientSecret, OneDriveRedirectUrl, accessToken);
                else
                    await connector.Reauthorize(clientId, clientSecret, OneDriveRedirectUrl, appSettings.OneDriveRefreshToken);
            }
            if (connector.isLoggedIn)
            {
                string folder = "";
                var files = await connector.ListFilesAsync(folder);
                //await connector.UploadFileAsync(file, "\\Pics");
            }
        }

        private string refreshToken = string.Empty;
        private string accessToken = string.Empty;


        private async void Connector_TokensChangedEvent(object sender, string e)
        {
            appSettings.OneDriveRefreshToken = (sender as OneDriveConnector).refreshToken;
            appSettings.OneDriveAccessToken = (sender as OneDriveConnector).accessToken;
            await SaveAsync(appSettings, fileName);
        }

        /// <summary>
        /// Generates the html for the OneDrive login page
        /// </summary>
        /// <returns></returns>
        public string GenerateOneDrivePage()
        {
            // Create OneDrive URL for logging in
            string uri = string.Format(OneDriveLoginUrl, clientId, OneDriveScope, OneDriveRedirectUrl);

            // Display login status
            string html = "<b>OneDrive Status:&nbsp;&nbsp;</b><span style='color:Red'>Not Logged In</span><br>";

            html += "<p class='sectionHeader'>Log into OneDrive:</p>";
            html += "<ol>";
            html += "<li>Click on this link:  <a href='" + uri + "' target='_blank'>OneDrive Login</a><br>" +
                "A new window will open.  Log into OneDrive.<br><br></li>";
            html += "<li>After you're done, you should arrive at a blank page.<br>" +
                "Copy the URL, paste it into this box, and click Submit.<br>" +
                "The URL will look something like this: https://login.live.com/oauth20_desktop.srf?code=M6b0ce71e-8961-1395-2435-f78db54f82ae&lc=1033 <br>" +
                " <form><input type='text' name='codeUrl' size='50'>  <input type='submit' value='Submit'></form></li>";
            html += "</ol><br><br>";

            return html;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            this.appSettings = await RestoreAsync(fileName);

            if (string.IsNullOrWhiteSpace(appSettings.OneDriveRefreshToken))
            {
                string html = GenerateOneDrivePage();
            }
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
            if (file != null && null != connector && connector.isLoggedIn)
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
                        await socketClient.Send(streamReader.ReadToEnd() + Environment.NewLine);
                    //await SocketClient.Disconnect();
                }
                //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));
            }
        }

        private void SocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine((e as StringMessageReceivedEventArgs).Message);
        }

        private async void button4_Click(object sender, RoutedEventArgs e)
        {
            if (socketClient != null)
            {
                await socketClient.Disconnect();
                socketClient = null;
            }


        }
    }
}