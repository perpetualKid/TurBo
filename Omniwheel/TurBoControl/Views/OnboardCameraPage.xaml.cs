using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TurBoControl.Controller;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TurBoControl.Views
{
    public sealed partial class OnboardCameraPage : Page
    {
        private ImageSourceController imageSource;

        public OnboardCameraPage()
        {
            this.InitializeComponent();
            imageSource = new ImageSourceController();
            imageSource.OnImageReceived += ImageSource_OnImageReceived;
        }

        private void ImageSource_OnImageReceived(object sender, EventArgs e)
        {
            this.imgMain.Source = imageSource.CurrentImage;
        }

        public ObservableCollection<BitmapImage> Items
        {
            get { return this.imageSource.CachedImages; }
        }

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            await imageSource.CaptureDeviceImage();
        }
    }
}
