using System;
using System.Threading.Tasks;
using Devices.Communication;
using Devices.Controllers.Base;
using Devices.Controllers.Common;
using Devices.Util.Extensions;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Turbo.Control.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {

        ApplicationDataContainer settings;
        private ImageSourceController imageSource;
        private GenericController testController;
        private GenericController driveController;

        public LandingPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            imageSource = await ImageSourceController.GetNamedInstance<ImageSourceController>(nameof(ImageSourceController), "FrontCamera");
            imageSource.OnImageReceived += ImageSource_OnImageReceived;
            testController = await GenericController.GetNamedInstance<GenericController>("LandingPage", "BrickPi.NxtColor.Port_S3");
            testController.OnResponseReceived += TestController_OnResponseReceived;
            this.imgPreview.Source = imageSource.CurrentImage ?? new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.png"));
            base.OnNavigatedTo(e);
        }

        private async void TestController_OnResponseReceived(object sender, JsonObject e)
        {
            switch (e.GetNamedString("Action"))
                {
                case "ARGB":
                    byte red = (byte)e.GetNamedNumber("Red");
                    byte green = (byte)e.GetNamedNumber("Green");
                    byte blue = (byte)e.GetNamedNumber("Blue");
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Color color = Color.FromArgb(0xFF, red, green, blue);
                        ellColor.Fill = new SolidColorBrush(color);
                    });
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            imageSource.OnImageReceived -= ImageSource_OnImageReceived;
            testController.OnResponseReceived -= TestController_OnResponseReceived;
            base.OnNavigatedFrom(e);
        }

        private async void ImageSource_OnImageReceived(object sender, BitmapImage e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.imgPreview.Source = e);
        }

        private async void Joypad_Moved(object sender, Controls.JoypadEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance} Angle: {e.Angle}";
            await DriveMove();
        }

        private async void Joypad_Released(object sender, Controls.JoypadEventArgs e)
        {
            await DriveStop();
        }

        private async void Joypad_Captured(object sender, EventArgs e)
        {
            await DriveStart();
        }

        private async void LinearSlider_Moved(object sender, Controls.SliderEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance}";
            await DriveMove();
        }

        private async void LinearSlider_Released(object sender, Controls.SliderEventArgs e)
        {
            await DriveStop();
        }

        private async void LinearSlider_Captured(object sender, EventArgs e)
        {
            await DriveStart();
        }

        private async Task DriveStart()
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                await testController.SendRequest("Start", "BrickPi.Drive");
            }
        }
        private async Task DriveStop()
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                await testController.SendRequest("Stop", "BrickPi.Drive");
            }
        }

        private async Task DriveMove()
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject move = new JsonObject();
                move.AddValue("Sender", "LandingPage");
                move.AddValue("Target", "BrickPi.Drive");
                move.AddValue("Action", "Move");
                move.AddValue("Direction", Joypad.Angle);
                move.AddValue("Velocity", Joypad.Distance);
                move.AddValue("Rotation", Slider.Distance);
                await testController.SendRequest(move, true);
            }
        }

        private async void imgPreview_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            await imageSource.CaptureDeviceImage();
        }

        private async void ellColor_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                await testController.SendRequest("ARGB");
            }
        }
    }
}
