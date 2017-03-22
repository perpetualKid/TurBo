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
        private bool updating;

        public OnboardCameraPage()
        {
            imageSource = ImageSourceController.GetNamedInstance<ImageSourceController>(nameof(ImageSourceController), "FrontCamera").Result;
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!await this.CheckConnection(e.Parameter))
                return;
            imageSource.OnImageReceived += ImageSource_OnImageReceived;
            imageSource.OnSupportedFormatsChanged += ImageSource_OnSupportedFormatsChanged;
            await imageSource.InitializeFormats();
            UpdateFormatSelectionElements(null);
            base.OnNavigatedTo(e);
        }

        private async void ImageSource_OnSupportedFormatsChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateFormatSelectionElements(sender);
            });
        }

        private void UpdateFormatSelectionElements(object sender)
        {

            if (!updating)
            {
                updating = true;
                object selectedValue;

                if (sender != dropdownFormatSelection)
                {
                    selectedValue = dropdownFormatSelection.SelectedValue;
                    dropdownFormatSelection.Items.Clear();
                    foreach (ImageSourceController.ImageFormat item in imageSource.GetSupportedFormatsFiltered(dropDownFormats.SelectedValue as string, dropDownResolutions.SelectedValue as string))
                    {
                        dropdownFormatSelection.Items.Add(item);
                    }
                    dropdownFormatSelection.SelectedValue = dropdownFormatSelection.Items.SingleOrDefault( item => item.Equals(imageSource.CurrentImageFormat.Value));
                }

                if (sender != dropDownFormats)
                {
                    selectedValue = dropDownFormats.SelectedValue;
                    dropDownFormats.Items.Clear();
                    dropDownFormats.Items.Add(string.Empty);
                    foreach (string format in imageSource.GetSupportedCaptureFormats(dropDownResolutions.SelectedValue as string))
                    {
                        dropDownFormats.Items.Add(format);
                    }
                    dropDownFormats.SelectedValue = selectedValue;
                }

                if (sender != dropDownResolutions)
                {
                    selectedValue = dropDownResolutions.SelectedValue;
                    dropDownResolutions.Items.Clear();
                    dropDownResolutions.Items.Add(string.Empty);
                    foreach (string resolutionString in imageSource.GetSupportedCaptureResolutions(dropDownFormats.SelectedValue as string))
                    {
                        dropDownResolutions.Items.Add(resolutionString);
                    }
                    dropDownResolutions.SelectedValue = selectedValue;
                }
                updating = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            imageSource.OnImageReceived -= ImageSource_OnImageReceived;
        }

        private async void ImageSource_OnImageReceived(object sender, BitmapImage e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.imgMain.Source = e);
        }

        public ObservableCollection<BitmapImage> Items
        {
            get { return this.imageSource.CachedImages; }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvPictureCache.SelectedIndex > -1)
                this.imgMain.Source = this.imageSource.CachedImages[lvPictureCache.SelectedIndex];
        }

        private void dropDownFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormatSelectionElements(sender);
        }

        private void dropDownResolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormatSelectionElements(sender);
        }

        private async void dropdownFormatSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageSourceController.ImageFormat? format = dropdownFormatSelection.SelectedValue as ImageSourceController.ImageFormat?;
            if (!updating && format.HasValue)
            {
                await imageSource.SetCaptureFormat(format.Value);
            }
        }

        private async void imgMain_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            await imageSource.CaptureDeviceImage();
        }

        private async void AppBarButtonCapturePicture_Click(object sender, RoutedEventArgs e)
        {
            await imageSource.CaptureDeviceImage();
        }
    }
}
