using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Devices.Util.Extensions;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Turbo.Control.UWP.Controller
{
    public class ImageSourceController
    {
        private static ImageSourceController instance;
        private ObservableCollection<BitmapImage> images;
        private BitmapImage currentImage;
        private const int maxImages = 10;

        public event EventHandler<EventArgs> OnImageReceived;

        private ImageSourceController()
        {
            images = new ObservableCollection<BitmapImage>();
            DeviceConnectionController.Instance.RegisterOnDataReceivedEvent(nameof(ImageSourceController), Instance_OnDataReceived);
        }

        public static ImageSourceController Instance
        {
            get
            {
                if (null == instance)
                    instance = new ImageSourceController();
                return instance;
            }
        }

        public ObservableCollection<BitmapImage> CachedImages
        {
            get { return this.images; }
        }

        public BitmapImage CurrentImage { get { return this.currentImage; } }

        public async Task CaptureDeviceImage()
        {
            JsonObject imageCapture = new JsonObject();
            imageCapture.AddValue("Target", "FrontCamera");
            imageCapture.AddValue("Action", "Capture");
            await DeviceConnectionController.Instance.Send(nameof(ImageSourceController), imageCapture);

        }

        private async void Instance_OnDataReceived(JsonObject data)
        {
            if (data.CompareKeyValue("Target", "FrontCamera") && data.CompareKeyValue("Action", "Capture"))
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

                    currentImage = image;
                    while (images.Count >= maxImages)
                        images.RemoveAt(maxImages - 1);
                    images.Insert(0, image);
                    OnImageReceived?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}
