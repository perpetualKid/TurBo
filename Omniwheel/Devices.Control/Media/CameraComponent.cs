using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Devices.Control.Media
{
    public class CameraComponent : Controllable
    {
        private MediaCapture mediaCapture;

        public CameraComponent(string componentName, MediaCapture mediaCapture) : base(componentName)
        {
            this.mediaCapture = mediaCapture;
        }

        protected override async Task InitializeDefaults()
        {
            await mediaCapture.InitializeAsync();
            await base.InitializeDefaults();
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "CAMERA HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "CAMERA ON|ENABLE : Turns the LED on.");
            data.AddMultiPartValue("Help", "CAMERA OFF|DISABLE : Turns the LED off.");
            data.AddMultiPartValue("Help", "CAMERA TOGGLE : Toggle the LED from current status.");
            data.AddMultiPartValue("Help", "CAMERA CAPTURE : Takes a picture and returns as .");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data).ConfigureAwait(false);
                    break;
                case "CAPTURE":
                    await CameraComponentCapture(data).ConfigureAwait(false);
                    break;
            }
        }

        #region command handling
        private async Task CameraComponentCapture(MessageContainer data)
        {
            string imageBase64 = string.Empty;
            using (IRandomAccessStream stream = await CaptureMediaStream().ConfigureAwait(false))
            {
                byte[] bytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                }
                imageBase64 = Convert.ToBase64String(bytes);
            }
            data.AddValue("ImageBase64", imageBase64);
            await HandleOutput(data).ConfigureAwait(false);
        }
        #endregion

        #region public
        public async Task<IRandomAccessStream> CaptureMediaStream()
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStreamAsync(imageProperties, stream).AsTask().ConfigureAwait(false);
            stream.Seek(0);
            return stream;
        }
        #endregion

        #region properties
        public MediaCapture MediaCapture { get { return this.mediaCapture; } }

        #endregion
    }
}
