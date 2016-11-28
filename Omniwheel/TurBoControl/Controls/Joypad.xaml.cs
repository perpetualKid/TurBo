using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace TurBoControl.Controls
{
    //Inspired from https://vjoystick.codeplex.com/

    public class JoypadEventArgs : EventArgs
    {
        public int Angle { get; set; }

        public int Distance { get; set; }
    }

    public sealed partial class Joypad : UserControl
    {
        /// <summary>Current angle in degrees from 0 to 360</summary>
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(int), typeof(Joypad), null);

        /// <summary>Current distance (or "force"), from 0 to 100</summary>
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(int), typeof(Joypad), null);

        /// <summary>Delta angle before Moved event is raised</summary>
        public static readonly DependencyProperty AngleChangeThresholdProperty =
            DependencyProperty.Register(nameof(AngleChangeThreshold), typeof(int), typeof(Joypad), null);

        /// <summary>Delta distance before Moved event is raised</summary>
        public static readonly DependencyProperty DistanceChangeThresholdProperty =
            DependencyProperty.Register(nameof(DistanceChangeThreshold), typeof(int), typeof(Joypad), null);

        /// <summary>Whether the joypad knob resets its place after being released</summary>
        public static readonly DependencyProperty DisableResetOnReleaseProperty =
            DependencyProperty.Register(nameof(DisableResetOnRelease), typeof(bool), typeof(Joypad), null);

        /// <summary>Current angle in degrees from 0 to 360</summary>
        public int Angle
        {
            get { return Convert.ToInt32(GetValue(AngleProperty)); }
            private set { SetValue(AngleProperty, value); }
        }

        /// <summary>current distance (or "power"), from 0 to 100</summary>
        public int Distance
        {
            get { return Convert.ToInt32(GetValue(DistanceProperty)); }
            private set { SetValue(DistanceProperty, value); }
        }

        /// <summary>Angle change delta before a Move events is raised</summary>
        public int AngleChangeThreshold
        {
            get { return Convert.ToInt32(GetValue(AngleChangeThresholdProperty)); }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 90)
                    value = 90;
                SetValue(AngleChangeThresholdProperty, value);
            }
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

        /// <summary>Indicates whether the joypad knob resets its place after being released</summary>
        public bool DisableResetOnRelease
        {
            get { return Convert.ToBoolean(GetValue(DisableResetOnReleaseProperty)); }
            set { SetValue(DisableResetOnReleaseProperty, value); }
        }

        /// <summary>This event fires whenever the joypad moves more than the threshold values</summary>
        public event EventHandler<JoypadEventArgs> Moved;

        /// <summary>This event fires once the joypad is released and its position is reset</summary>
        public event EventHandler<JoypadEventArgs> Released;

        /// <summary>This event fires once the joypad is captured</summary>
        public event EventHandler Captured;

        private Point startPosition;
        private int previousAngle, previousDistance;
        private bool pointerCaptured;
        private const double radian2Degree = 180 / Math.PI;

        public Joypad()
        {
            this.InitializeComponent();
            Knob.PointerPressed += Knob_PointerPressed;
            Knob.PointerReleased += Knob_PointerReleased;
            Knob.PointerMoved += Knob_PointerMoved;
            centerKnob = Knob.Resources["CenterKnob"] as Storyboard;
        }

        private void Knob_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Knob.ReleasePointerCapture(e.Pointer);
            pointerCaptured = false;
            Released?.Invoke(this, new JoypadEventArgs { Angle = Angle, Distance = Distance });
            if (!this.DisableResetOnRelease)
            {
                centerKnob.Begin();
            }
        }

        private void Knob_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            startPosition = e.GetCurrentPoint(Base).Position;
            startPosition.X -= knobPosition.X;
            startPosition.Y -= knobPosition.Y;

            Captured?.Invoke(this, EventArgs.Empty);
            pointerCaptured = Knob.CapturePointer(e.Pointer);

        }

        private void CenterKnob_Completed(object sender, object e)
        {
            Angle = Distance = previousAngle = previousDistance = 0;
            Moved?.Invoke(this, new JoypadEventArgs { Angle = Angle, Distance = Distance });
        }

        private void Knob_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!pointerCaptured)
                return;

            Point newPos = e.GetCurrentPoint(Base).Position;
            Point deltaPos = new Point(newPos.X - startPosition.X, newPos.Y - startPosition.Y);

            if (deltaPos.X == 0 && deltaPos.Y == 0)
                return; //shortcut

            double angle = Math.Atan2(deltaPos.Y, deltaPos.X);

            int distance = (int)Math.Round(Math.Sqrt(deltaPos.X * deltaPos.X + deltaPos.Y * deltaPos.Y) / 135 * 100);
            if (distance > 100)
            {
                distance = 100;
                deltaPos.X = 135 * Math.Cos(angle);
                deltaPos.Y = 135 * Math.Sin(angle);
            }

            angle *= -radian2Degree;    //counterclockwise
            angle = Math.Round((270 + angle) % 360); //turn 90deg to ensure 0 is up, and values are 0-359

            Angle = (int)angle;
            Distance = distance;

            knobPosition.X = deltaPos.X;
            knobPosition.Y = deltaPos.Y;

            if ((Math.Abs(previousAngle - angle) < AngleChangeThreshold) && (Math.Abs(previousDistance - distance) < DistanceChangeThreshold))
                return;

            Moved?.Invoke(this, new JoypadEventArgs { Angle = Angle, Distance = Distance });

            previousAngle = Angle;
            previousDistance = Distance;
        }

    }
}
