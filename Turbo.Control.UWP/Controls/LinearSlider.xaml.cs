using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Turbo.Control.UWP.Controls
{
    public class SliderEventArgs : EventArgs
    {
        public int Distance { get; set; }
    }

    public sealed partial class LinearSlider : UserControl
    {

        /// <summary>Current distance (or "force"), from 0 to 100</summary>
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(int), typeof(LinearSlider), null);

        /// <summary>Delta distance before Moved event is raised</summary>
        public static readonly DependencyProperty DistanceChangeThresholdProperty =
            DependencyProperty.Register(nameof(DistanceChangeThreshold), typeof(int), typeof(LinearSlider), null);

        /// <summary>Whether the slider resets its place after being released</summary>
        public static readonly DependencyProperty ResetOnReleaseProperty =
            DependencyProperty.Register(nameof(ResetOnRelease), typeof(bool), typeof(LinearSlider), new PropertyMetadata(true));

        /// <summary>current distance (or "power"), from 0 to 100</summary>
        public int Distance
        {
            get { return Convert.ToInt32(GetValue(DistanceProperty)); }
            private set { SetValue(DistanceProperty, value); }
        }

        /// <summary>Distance change delta before a Move event is raised </summary>
        public int DistanceChangeThreshold
        {
            get { return Convert.ToInt32(GetValue(DistanceChangeThresholdProperty)); }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 50)
                    value = 50;
                SetValue(DistanceChangeThresholdProperty, value);
            }
        }

        public bool ResetOnRelease
        {
            get { return Convert.ToBoolean(GetValue(ResetOnReleaseProperty)); }
            set { SetValue(ResetOnReleaseProperty, value); }
        }

        /// <summary>This event fires whenever the slider moves more than the threshold values</summary>
        public event EventHandler<SliderEventArgs> Moved;

        /// <summary>This event fires once the slider is released and its position is reset</summary>
        public event EventHandler<SliderEventArgs> Released;

        /// <summary>This event fires once the slider is captured</summary>
        public event EventHandler Captured;

        private Point startPosition;
        private int previousDistance;
        private bool pointerCaptured;

        public LinearSlider()
        {
            this.InitializeComponent();
            Slider.PointerPressed += Slider_PointerPressed;
            Slider.PointerReleased += Slider_PointerReleased;
            Slider.PointerMoved += Slider_PointerMoved;
            centerSlider = Slider.Resources["CenterSlider"] as Storyboard;

        }

        private void Slider_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!pointerCaptured)
                return;

            Point newPos = e.GetCurrentPoint(Base).Position;
            double delta = newPos.X - startPosition.X;

            if (delta == 0)
                return; //shortcut

            int distance = (int)(delta / 185 * 100); //mapping distance to 0-100 range
            if (Math.Abs(distance) > 100)
            {
                distance = 100 * Math.Sign(distance);
                delta = distance * 185 / 100;
            }
            Distance = distance;

            sliderPosition.X = delta;

            if (Math.Abs(previousDistance - distance) < DistanceChangeThreshold)
                return;

            Moved?.Invoke(this, new SliderEventArgs { Distance = Distance });

            previousDistance = Distance;
        }

        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Slider.ReleasePointerCapture(e.Pointer);
            pointerCaptured = false;
            Released?.Invoke(this, new SliderEventArgs { Distance = Distance });
            if (this.ResetOnRelease)
            {
                centerSlider.Begin();
            }
        }

        private void Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            startPosition = e.GetCurrentPoint(Base).Position;
            startPosition.X -= sliderPosition.X;
            startPosition.Y -= sliderPosition.Y;

            Captured?.Invoke(this, EventArgs.Empty);
            pointerCaptured = Slider.CapturePointer(e.Pointer);
        }

        private void CenterSlider_Completed(object sender, object e)
        {
            Distance = previousDistance = 0;
            Moved?.Invoke(this, new SliderEventArgs { Distance = Distance });
        }

    }
}
