using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devices.Components;
using Devices.Util.Extensions;
using Windows.Data.Json;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Turbo.BrickPi.Components.Media
{
    public class CameraComponent : ComponentBase
    {
        private MediaCapture mediaCapture;
        private MediaCaptureInitializationSettings mediaCaptureSettings;
        private IEnumerable<IMediaEncodingProperties> supportedFormats;

        public CameraComponent(string componentName, MediaCapture mediaCapture) : base(componentName)
        {
            this.mediaCapture = mediaCapture;
        }

        public CameraComponent(string componentName, MediaCaptureInitializationSettings mediaCaptureSettings) : base(componentName)
        {
            this.mediaCaptureSettings = mediaCaptureSettings;

        }

        protected override async Task InitializeDefaults()
        {
            if (null == mediaCapture)
                mediaCapture = new MediaCapture();
            if (null != mediaCaptureSettings)
            {
                await mediaCapture.InitializeAsync(mediaCaptureSettings).AsTask().ConfigureAwait(false);
            }
            else
            {
                await mediaCapture.InitializeAsync().AsTask().ConfigureAwait(false);
            }
            await base.InitializeDefaults();
        }

        #region command handling
        [Action("Capture")]
        [ActionHelp("Takes a picture and returns as Base64 string.")]
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
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("GetCurrentFormat")]
        [ActionHelp("Returns current capture format.")]
        private async Task CameraComponentListProperties(MessageContainer data)
        {
            data.AddMultiPartValue("MediaFormat", VideoPropertiesToJson(await GetCurrentFormat() as VideoEncodingProperties));
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        private async Task CameraComponentSetResolution(MessageContainer data)
        {
            await Task.CompletedTask;
            uint width = uint.Parse(data.ResolveParameter("Width", 0));
            uint height = uint.Parse(data.ResolveParameter("Height", 1));
            //await SetCurrentFormat(width, height).ConfigureAwait(false);
        }

        [Action("ListFormats")]
        [Action("GetAllFormats")]
        [ActionParameter("Type", Required = false)]
        [ActionParameter("SubType", Required = false)]
        [ActionParameter("Width", Required = false)]
        [ActionParameter("Height", Required = false)]
        [ActionParameter("BitRate", Required = false)]
        [ActionHelp("Returns a list of available capture formats. If provided, filtered by the given parameters")]
        private async Task CameraComponentGetAllFormats(MessageContainer data)
        {
            uint width;
            uint height;
            uint bitrate;
            string type = data.ResolveParameter("Type", 0);
            string subtype = data.ResolveParameter("Subtype", 1);
            if (!uint.TryParse(data.ResolveParameter("Width", 2), out width))
                width = 0;
            if (!uint.TryParse(data.ResolveParameter("Height", 3), out height))
                height = 0;
            if (!uint.TryParse(data.ResolveParameter("BitRate", 4), out bitrate))
                bitrate = 0;

            foreach (var item in await GetSupportedMediaFormats(type, subtype, width, height, bitrate).ConfigureAwait(false))
            {
                data.AddMultiPartValue("MediaFormat", VideoPropertiesToJson(item as VideoEncodingProperties));
            }
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
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

        public async Task<IEnumerable<IMediaEncodingProperties>> GetSupportedMediaFormats()
        {
            if (null != supportedFormats)
                return await Task.FromResult<IEnumerable<IMediaEncodingProperties>>(supportedFormats);
            return await Task.Run(() =>
           {
               supportedFormats = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).OrderByDescending(
         resolution => ((VideoEncodingProperties)resolution).Width);
               return supportedFormats;
           });
        }

        public async Task<IMediaEncodingProperties> GetCurrentFormat()
        {
            return await Task.Run(() => mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.Photo));
        }

        public async Task SetCurrentFormat(IMediaEncodingProperties format)
        {
            await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, format).AsTask().ConfigureAwait(false);
        }

        public async Task<IEnumerable<IMediaEncodingProperties>> GetSupportedMediaFormats(string type, string subType, uint width, uint height, uint bitrate)
        {
            IEnumerable<IMediaEncodingProperties> formats = await GetSupportedMediaFormats().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(type))
                formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).Type.ToLowerInvariant() == type.ToLowerInvariant());
            if (!string.IsNullOrWhiteSpace(subType))
                formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).Subtype.ToLowerInvariant() == subType.ToLowerInvariant());
            if (width > 0)
                formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).Width == width);
            if (height > 0)
                formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).Height == height);
            if (bitrate> 0)
                formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).Bitrate == bitrate);
            //if (bitrate > 0)
            //    formats = formats.Where(videoFormat => ((VideoEncodingProperties)videoFormat).FrameRate. == bitrate);

            return formats;
        }

        #endregion

        #region properties
        public MediaCapture MediaCapture { get { return this.mediaCapture; } }

        #endregion

        #region private helpers
        private JsonObject VideoPropertiesToJson(VideoEncodingProperties videoProperties)
        {
            JsonObject properties = new JsonObject();
            properties.AddValue(nameof(videoProperties.Bitrate), videoProperties.Bitrate);
            properties.AddValue(nameof(videoProperties.FrameRate), $"{videoProperties.FrameRate.Denominator}/{videoProperties.FrameRate.Numerator}");
            properties.AddValue(nameof(videoProperties.Height), videoProperties.Height);
            properties.AddValue(nameof(videoProperties.ProfileId), videoProperties.ProfileId);
            properties.AddValue(nameof(videoProperties.PixelAspectRatio), $"{videoProperties.PixelAspectRatio.Denominator}/{videoProperties.PixelAspectRatio.Numerator}");
            properties.AddValue(nameof(videoProperties.Subtype), videoProperties.Subtype);
            properties.AddValue(nameof(videoProperties.Type), videoProperties.Type);
            properties.AddValue(nameof(videoProperties.Width), videoProperties.Width);
            return properties;
        }
        #endregion
    }
}
