using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors.NXT;
using Common.Base;
using Common.Communication;
using Devices.Control.Communication;
using Devices.Control.Lego;
using Devices.Control.Media;
using Devices.Control.Storage;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace OmniWheel
{

    public sealed class StartupTask : IBackgroundTask
    {
        NXTTouchSensor touch;
        NXTColorSensor color;
        MediaCapture mediaCapture;
        private StorageFile photoFile;
        private readonly string PHOTO_FILE_NAME = "Camera Roll\\photo.jpg";

        private OneDriveComponent oneDrive;
        private AzureBlobStorageComponent azureBlob;
        private BrickPiComponent brickComponent;
        private Brick brick;
        private DriveComponent omniDrive;
        private CameraComponent camera;
//        int counter;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            this.appSettings = await RestoreAsync(fileName).ConfigureAwait(false);
            List<Task> setupTasks = new List<Task>();

            setupTasks.Add(Controllable.RegisterComponent(new NetworkListener(8027)));
            setupTasks.Add(Controllable.RegisterComponent(new NetworkListener(8031, DataFormat.Json)));
            oneDrive = new OneDriveComponent();
            setupTasks.Add(Controllable.RegisterComponent(oneDrive));
            azureBlob = new AzureBlobStorageComponent();
            setupTasks.Add(Controllable.RegisterComponent(azureBlob));
            brickComponent = new BrickPiComponent();
            setupTasks.Add(Controllable.RegisterComponent(brickComponent));
            await Task.WhenAll(setupTasks).ConfigureAwait(false);

            brick = brickComponent.BrickPi;
            Debug.WriteLine($"Brick Version: {await brick.GetBrickVersion().ConfigureAwait(false)}");

            touch = new NXTTouchSensor(SensorPort.Port_S1);
            touch.OnPressed += Touch_OnPressed;
            await brick.Sensors.Add(touch).ConfigureAwait(false);
            color = new NXTColorSensor(SensorPort.Port_S4, SensorType.COLOR_FULL);
            await brick.Sensors.Add(color).ConfigureAwait(false);
            color = new NXTColorSensor(SensorPort.Port_S3, SensorType.COLOR_FULL);
            await brick.Sensors.Add(color).ConfigureAwait(false);
            await brickComponent.RegisterSensors();
            omniDrive = new DriveComponent("Drive", brickComponent, brick.Motors[MotorPort.Port_MA], brick.Motors[MotorPort.Port_MD], brick.Motors[MotorPort.Port_MB]);
            await Controllable.RegisterComponent(omniDrive).ConfigureAwait(false);

            camera = new CameraComponent("FrontCamera", new MediaCapture());
            await Controllable.RegisterComponent(camera).ConfigureAwait(false);

            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture).AsTask<DeviceInformationCollection>().ConfigureAwait(false);
            mediaCapture = new MediaCapture();


            //await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            //{
            //    StreamingCaptureMode = StreamingCaptureMode.Video,
            //    PhotoCaptureSource = PhotoCaptureSource.Auto,
            //    //AudioDeviceId = string.Empty,
            //    VideoDeviceId = allVideoDevices[0].Id
            //}).AsTask().ConfigureAwait(false);

            //var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).OrderByDescending( resolution => ((VideoEncodingProperties)resolution).Width);
            //foreach (var item in resolutions)
            //{
            //    Debug.WriteLine((item as VideoEncodingProperties).Width);
            //}

            //await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolutions.First());



            brick.Start();
            while (true)
            {
                brick.Arduino1Led.Toggle();
//                Debug.WriteLine($"Color Raw:{color.RawValue} Name {color.ColorName} ARGB: {color.ColorData}");
                await Task.Delay(500).ConfigureAwait(false);
            }
        }

        private async void Touch_OnPressed(object sender, BrickPi.Uwp.Base.SensorEventArgs e)
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            //await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile).AsTask().ConfigureAwait(false);

            await mediaCapture.CapturePhotoToStreamAsync(imageProperties, stream).AsTask().ConfigureAwait(false);
            stream.Seek(0);

            await oneDrive.UploadFile(stream, "/Pics", photoFile.Name);
            
            //CloudBlockBlob blockBlob = container.GetBlockBlobReference(photoFile.Name);
            //await blockBlob.UploadFromStreamAsync(stream.AsStreamForRead()).ConfigureAwait(false);
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

        //private async void Connector_TokensChangedEvent(object sender, EventArgs e)
        //{
        //    appSettings.OneDriveRefreshToken = (sender as OneDriveConnector).RefreshToken;
        //    appSettings.OneDriveAccessToken = (sender as OneDriveConnector).AccessToken;
        //    await SaveAsync(appSettings, fileName);
        //}

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

