using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace TurBoControl.Controls
{
    public class JoypadEventArgs : EventArgs
    {
        public double Angle { get; set; }

        public double Distance { get; set; }
    }

    public sealed partial class Joypad : UserControl
    {
        /// <summary>Current angle in degrees from 0 to 360</summary>

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(Joypad), null);

        /// <summary>Current distance (or "power"), from 0 to 100</summary>
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(double), typeof(Joypad), null);

        /// <summary>How often should be raised StickMove event in degrees</summary>
        public static readonly DependencyProperty AngleStepProperty =
            DependencyProperty.Register(nameof(AngleMoveThreshold), typeof(double), typeof(Joypad), new PropertyMetadata(1.0));

        /// <summary>How often should be raised StickMove event in distance units</summary>
        public static readonly DependencyProperty DistanceStepProperty =
            DependencyProperty.Register(nameof(DistanceMoveThreshold), typeof(double), typeof(Joypad), new PropertyMetadata(1.0));

        /// <summary>Indicates whether the joystick knob resets its place after being released</summary>
        public static readonly DependencyProperty ResetKnobAfterReleaseProperty =
            DependencyProperty.Register(nameof(ResetKnobAfterRelease), typeof(bool), typeof(Joypad), new PropertyMetadata(true));

        /// <summary>Current angle in degrees from 0 to 360</summary>
        public double Angle
        {
            get { return Convert.ToDouble(GetValue(AngleProperty)); }
            private set { SetValue(AngleProperty, value); }
        }

        /// <summary>current distance (or "power"), from 0 to 100</summary>
        public double Distance
        {
            get { return Convert.ToDouble(GetValue(DistanceProperty)); }
            private set { SetValue(DistanceProperty, value); }
        }

        /// <summary>Movement Delta Threshold before Events are raised</summary>
        public double AngleMoveThreshold
        {
            get { return Convert.ToDouble(GetValue(AngleStepProperty)); }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 90)
                    value = 90;
                SetValue(AngleStepProperty, Math.Round(value));
            }
        }

        /// <summary>How often should be raised StickMove event in distance units</summary>
        public double DistanceMoveThreshold
        {
            get { return Convert.ToDouble(GetValue(DistanceStepProperty)); }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 50)
                    value = 50;
                SetValue(DistanceStepProperty, value);
            }
        }

        /// <summary>Indicates whether the joypad knob resets its place after being released</summary>
        public bool ResetKnobAfterRelease
        {
            get { return Convert.ToBoolean(GetValue(ResetKnobAfterReleaseProperty)); }
            set { SetValue(ResetKnobAfterReleaseProperty, value); }
        }

        /// <summary>This event fires whenever the joypad moves</summary>
        public event EventHandler<JoypadEventArgs> Moved;

        /// <summary>This event fires once the joypad is released and its position is reset</summary>
        public event EventHandler<JoypadEventArgs> Released;

        /// <summary>This event fires once the joypad is captured</summary>
        public event EventHandler Captured;

        private Point startPosition;
        private double previousAngle, previousDistance;
        private bool pointerCaptured;

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
            centerKnob.Begin();
        }

        private void Knob_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            startPosition = e.GetCurrentPoint(Base).Position;
            previousAngle = previousDistance = 0;

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

            double distance = Math.Round(Math.Sqrt(deltaPos.X * deltaPos.X + deltaPos.Y * deltaPos.Y) / 135 * 100);
            if (distance > 100)
            {
                distance = 100;
                deltaPos.X = 135 * Math.Cos(angle);
                deltaPos.Y = 135 * Math.Sin(angle);
            }

            angle = angle * 180 / Math.PI;
            angle = Math.Round((angle + 450) % 360); //turn 90deg to ensure 0 is up, and values are 0-359

            Angle = angle;
            Distance = distance;

            knobPosition.X = deltaPos.X;
            knobPosition.Y = deltaPos.Y;

            if ((Math.Abs(previousAngle - angle) < AngleMoveThreshold) && (Math.Abs(previousDistance - distance) < DistanceMoveThreshold))
                return;

            Moved?.Invoke(this, new JoypadEventArgs { Angle = Angle, Distance = Distance });

            previousAngle = Angle;
            previousDistance = Distance;
        }

    }
}
