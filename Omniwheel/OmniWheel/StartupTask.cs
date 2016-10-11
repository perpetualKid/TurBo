using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using BrickPi.Uwp;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using BrickPi.Uwp.Sensors.NXT;
using Windows.Storage;
using BrickPi.Uwp.Base;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Windows.Storage.Streams;
using System.Xml.Serialization;
using Common.Communication.Channels;
using Common.Communication;
using Devices.Control.Communication;
using Devices.Control.Base;
using OneDrive;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace OmniWheel
{
    public sealed class StartupTask : IBackgroundTask
    {
        NXTTouchSensor touch;
        MediaCapture mediaCapture;
        private StorageFile photoFile;
        private readonly string PHOTO_FILE_NAME = "Camera Roll\\photo.jpg";
        string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=turtlebot;AccountKey=761qXiS3qdsDoA9X7j7yqPi/bAdALBEvuSirEjkeL4cZPTko0A7qmM2puwwYquyoxCw8HFP+htPHIkG06dwsHg==";
        CloudBlobContainer container;
//        int counter;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            this.appSettings = await RestoreAsync(fileName);

//            await OneDriveConnect();

            //            connector = new OmniWheel.OneDriveConnector();
            //            connector.TokensChangedEvent += Connector_TokensChangedEvent;
            //            accessToken = "Mbfd0a4b1-7543-f3ce-d199-991c52273591";
            ////            await connector.LoginAsync(clientId, clientSecret, redirectUrl, accessToken);

            //            await connector.Reauthorize(clientId, clientSecret, redirectUrl, refreshToken);

            await ControllableComponent.RegisterComponent(new NetworkListener(8027)).ConfigureAwait(false);
            await ControllableComponent.RegisterComponent(new NetworkListener(8029)).ConfigureAwait(false);
            await ControllableComponent.RegisterComponent(new OneDriveComponent()).ConfigureAwait(false);
            //channel = await SocketServer.AddChannel(8027, DataFormat.StringText);
            ////await SocketServer.Instance(8027).AddChannel(DataFormat.String);
            //channel.OnMessageReceived += StartupTask_OnStringMessageReceived;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container.
            container = blobClient.GetContainerReference("turtlebot");
            // Create the container if it doesn't already exist.
            //await container.CreateIfNotExistsAsync();
            //await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });

            Brick brick = await Brick.InitializeInstance("Uart0");
            int version = await brick.GetBrickVersion();
            Debug.WriteLine("Brick Version: {0}", version);

            touch = new NXTTouchSensor(SensorPort.Port_S1);
            touch.OnPressed += Touch_OnPressed;
            await brick.Sensors.Add(touch);

            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            brick.Start();
            while (true)
            {
                brick.Arduino1Led.Toggle();
                await Task.Delay(500);
            }
        }

        //private async void StartupTask_OnStringMessageReceived(object sender, MessageReceivedEventArgs e)
        //{
        //    Debug.WriteLine((e as StringMessageReceivedEventArgs).Message);
        //    await channel.Send(counter++.ToString());
        //}

        private async void Touch_OnPressed(object sender, BrickPi.Uwp.Base.SensorEventArgs e)
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile).AsTask().ConfigureAwait(false);

            //await OneDrive(photoFile);

            await mediaCapture.CapturePhotoToStreamAsync(imageProperties, stream).AsTask().ConfigureAwait(false);
            stream.Seek(0);

            
            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(photoFile.Name);

            // Create or overwrite the "myblob" blob with contents from a local file.
            await blockBlob.UploadFromStreamAsync(stream.AsStreamForRead()).ConfigureAwait(false);
        }


        private string refreshToken = string.Empty;
        private string accessToken = string.Empty;

        //private async Task OneDrive(StorageFile file)
        //{
        //    if (null == connector || !connector.LoggedIn)
        //        await OneDriveConnect();
        //    if (connector.LoggedIn)
        //    {
        //        await connector.UploadFileAsync(file, "/Pics");
        //    }
        //}


        private async void Connector_TokensChangedEvent(object sender, EventArgs e)
        {
            appSettings.OneDriveRefreshToken = (sender as OneDriveConnector).RefreshToken;
            appSettings.OneDriveAccessToken = (sender as OneDriveConnector).AccessToken;
            await SaveAsync(appSettings, fileName);
        }

        private AppSettings appSettings;

        private static readonly StorageFolder SettingsFolder = ApplicationData.Current.LocalFolder;
        private string fileName = "Settings.xml";

        /// <summary>
        /// Save the settings to a file
        /// </summary>
        /// <param name="settings">Settings object to save</param>
        /// <param name="filename">Name of file to save to</param>
        /// <returns></returns>
        private static async Task SaveAsync(AppSettings settings, string filename)
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
        private static async Task<AppSettings> RestoreAsync(string filename)
        {
            try
            {
                //var test = await SettingsFolder.TryGetItemAsync(filename);
                //if (test != null)
                //{
                //    using (Stream readStream = await SettingsFolder.OpenStreamForReadAsync(filename))
                //    {
                //        var serializer = new XmlSerializer(typeof(AppSettings));
                //        //                    readStream.Seek(0, SeekOrigin.Begin);
                //        //                    return (AppSettings)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                //        var temp = serializer.Deserialize(readStream);
                //        return (AppSettings)temp;
                //    }
                //}
                StorageFile sessionFile = await SettingsFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                if (sessionFile == null)
                { 
                    AppSettings result = new AppSettings();
                    await SaveAsync(result, filename);
                    return result;
                }
//                using (IInputStream sessionInputStream = await sessionFile.OpenReadAsync())
                using (Stream readStream = await sessionFile.OpenStreamForReadAsync())
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
//                    readStream.Seek(0, SeekOrigin.Begin);
//                    return (AppSettings)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                    var temp = serializer.Deserialize(readStream);
                    return (AppSettings)temp;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AppSettings.RestoreAsync(): " + ex.Message);
                // If settings.xml file is corrupted and cannot be read - behave as if it does not exist.
                AppSettings result = new AppSettings();
                await SaveAsync(result, filename);
                return result;
            }
        }
    }

    public struct AppSettings
    {
        // Obtained from OneDrive Login
        public string OneDriveAccessToken;
        public string OneDriveRefreshToken;
    }
}

