using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Devices.Controllers.Base;
using Devices.Controllers.Common;
using Turbo.Control.UWP.Util;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using Windows.Data.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Turbo.Control.UWP.Views
{
    public sealed partial class OnboardCameraPage : Page
    {
        private ImageSourceController imageSource;

        public OnboardCameraPage()
        {
            this.InitializeComponent();
            imageSource = ImageSourceController.GetNamedInstance<ImageSourceController>(nameof(ImageSourceController)).Result;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!await this.ConnectionNeeded(typeof(LandingPage), e.Parameter))
                return;
            base.OnNavigatedTo(e);
            imageSource.OnImageReceived += ImageSource_OnImageReceived;
            imageSource.OnCurrentFormatChanged += ImageSource_OnFormatChanged;
            imageSource.OnSupportedFormatsChanged += ImageSource_OnSupportedFormatsChanged;
            await imageSource.GetCurrentFormat();
            await imageSource.GetSupportedFormats();
        }

        private async void ImageSource_OnSupportedFormatsChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateFormatSelectionElements(sender);
            });
        }

        private async void ImageSource_OnFormatChanged(object sender, ImageSourceController.ImageFormat imageFormat)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Text = imageFormat.ToString();
            });
        }

        private async void SupportedFormats_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateFormatSelectionElements(sender);
            });
        } 

        private void UpdateFormatSelectionElements(object sender)
        {
            dropdownAllFormats.Items.Clear();
            foreach (string item in imageSource.GetAllSupportedFormats())
            {
                dropdownAllFormats.Items.Add(item);
            }

            if (sender != dropDownFormats)
            {
                dropDownFormats.Items.Clear();
                dropDownFormats.Items.Add(string.Empty);
                foreach (string format in imageSource.GetSupportedCaptureFormats())
                {
                    dropDownFormats.Items.Add(format);
                }
            }

            if (sender != dropDownResolutions)
            {
                dropDownResolutions.Items.Clear();
                foreach (string resolutionString in imageSource.GetSupportedCaptureResolutions(dropDownFormats.SelectedValue as string))
                {
                    dropDownResolutions.Items.Add(resolutionString);
                }
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            imageSource.OnImageReceived -= ImageSource_OnImageReceived;
            imageSource.OnCurrentFormatChanged -= ImageSource_OnFormatChanged;
        }

        private async void ImageSource_OnImageReceived(object sender, BitmapImage e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.imgMain.Source = e);
        }

        public ObservableCollection<BitmapImage> Items
        {
            get { return this.imageSource.CachedImages; }
        }

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            await imageSource.CaptureDeviceImage();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvPictureCache.SelectedIndex > -1)
                this.imgMain.Source = this.imageSource.CachedImages[lvPictureCache.SelectedIndex];
        }

        private async void btnLoadFormats_Click(object sender, RoutedEventArgs e)
        {
            await imageSource.GetSupportedFormats(subType:dropDownFormats.SelectedValue.ToString());
        }

        private void dropDownFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormatSelectionElements(sender);
        }
    }
}
