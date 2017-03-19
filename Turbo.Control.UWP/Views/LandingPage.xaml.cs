using System;
using System.Threading.Tasks;
using Devices.Communication;
using Devices.Controllers.Base;
using Devices.Util.Extensions;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Turbo.Control.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {

        ApplicationDataContainer settings;

        public LandingPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Instance_OnDataReceived(JsonObject data)
        {
            if (data.ContainsKey("Target") && data.GetNamedString("Target").ToUpperInvariant() == "BrickPi.NxtColor.Port_S3".ToUpperInvariant() &&
                data.ContainsKey("Action") && data.GetNamedString("Action").ToUpperInvariant() == "ARGB")
            {
                //string status = data.GetNamedString("Status");
                byte red = (byte)data.GetNamedNumber("Red");
                byte green = (byte)data.GetNamedNumber("Green");
                byte blue = (byte)data.GetNamedNumber("Blue");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Color color = Color.FromArgb(0xFF, red, green, blue);
                    ellColor.Fill = new SolidColorBrush(color);
                    //if (status == "Enabled")
                    //{
                    //    btnConnect.Background = new SolidColorBrush(Colors.Blue);
                    //}
                    //else
                    //    btnConnect.Background = new SolidColorBrush(Colors.Gray);
                });

            }
        }

        private async void Joypad_Moved(object sender, Controls.JoypadEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance} Angle: {e.Angle}";
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject move = new JsonObject();
                move.AddValue("Target", "BrickPi.Drive");
                move.AddValue("Action", "Move");
                move.AddValue("Direction", e.Angle);
                move.AddValue("Velocity", e.Distance);
                move.AddValue("Rotation", Slider.Distance);
                await ControllerHandler.Send("LandingPage", move);

            }
        }

        private async void Joypad_Released(object sender, Controls.JoypadEventArgs e)
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject stop = new JsonObject();
                stop.AddValue("Target", "BrickPi.Drive");
                stop.AddValue("Action", "Stop");
                await ControllerHandler.Send("LandingPage", stop);

            }
        }

        private async void Joypad_Captured(object sender, EventArgs e)
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject start = new JsonObject();
                start.AddValue("Target", "BrickPi.Drive");
                start.AddValue("Action", "Start");
                await ControllerHandler.Send("LandingPage", start);

            }

        }

        private async void LinearSlider_Moved(object sender, Controls.SliderEventArgs e)
        {
            JoypadValues.Text = $"Force: {e.Distance}";
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject move = new JsonObject();
                move.AddValue("Target", "BrickPi.Drive");
                move.AddValue("Action", "Move");
                move.AddValue("Direction", Joypad.Angle);
                move.AddValue("Velocity", Joypad.Distance);
                move.AddValue("Rotation", e.Distance);
                await ControllerHandler.Send("LandingPage", move);
            }
        }

        private async void LinearSlider_Released(object sender, Controls.SliderEventArgs e)
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject stop = new JsonObject();
                stop.AddValue("Target", "BrickPi.Drive");
                stop.AddValue("Action", "Stop");
                await ControllerHandler.Send("LandingPage", stop);

            }
        }

        private async void LinearSlider_Captured(object sender, EventArgs e)
        {
            if (ControllerHandler.ConnectionStatus == ConnectionStatus.Connected)
            {
                JsonObject start = new JsonObject();
                start.AddValue("Target", "BrickPi.Drive");
                start.AddValue("Action", "Start");
                await ControllerHandler.Send("LandingPage", start);

            }
        }
    }
}
