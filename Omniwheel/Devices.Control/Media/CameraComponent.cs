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
            data.AddMultiPartValue("Help", "CAMERA CAPTURE : Takes a picture and returns as Base64 string.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (data.ResolveParameter(nameof(MessageContainer.FixedPropertyNames.Action), 1).ToUpperInvariant())
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
            using (IRandomAccessStream stream = await CaptureMediaStream(ImageEncodingProperties.CreateJpeg()).ConfigureAwait(false))
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
        public async Task<IRandomAccessStream> CaptureMediaStream(ImageEncodingProperties encoding)
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            ImageEncodingProperties imageProperties = encoding;
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
