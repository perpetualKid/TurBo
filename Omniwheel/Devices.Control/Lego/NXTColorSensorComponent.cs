using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors.NXT;
using Common.Base;

namespace Devices.Control.Lego
{
    public class NXTColorSensorComponent : Controllable
    {
        private NXTColorSensor colorSensor;

        public NXTColorSensorComponent(string componentName, Controllable parent, NXTColorSensor colorSensor) : base(componentName, parent)
        {
            this.colorSensor = colorSensor;
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "NXTColor HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "NXTColor ARGB : Returns the Ambient Red Green Blue values.");
            data.AddMultiPartValue("Help", "NXTColor RAW|RAWVALUE : Returns the raw sensor value.");
            data.AddMultiPartValue("Help", "NXTColor COLORNAME: Returns the color name.");
            data.AddMultiPartValue("Help", "NXTColor MODE : Returns the current working mode for this sensor.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data).ConfigureAwait(false);
                    break;
                case "ARGB":
                    await ColorSensorGetARGB(data).ConfigureAwait(false);
                    break;
                case "RAW":
                case "RAWVALUE":
                    await ColorSensorGetRawValue(data).ConfigureAwait(false);
                    break;
                case "COLORNAME":
                    await ColorSensorGetColorName(data).ConfigureAwait(false);
                    break;
                case "MODE":
                    await ColorSensorGetMode(data).ConfigureAwait(false);
                    break;
            }
        }

        #region command handling
        private async Task ColorSensorGetMode(MessageContainer data)
        {
            data.AddValue("Mode", colorSensor.SensorType.ToString());
            await HandleOutput(data).ConfigureAwait(false);
        }

        private async Task ColorSensorGetColorName(MessageContainer data)
        {
            data.AddValue(nameof(colorSensor.ColorName), colorSensor.ColorName);
            await HandleOutput(data).ConfigureAwait(false);
        }
        private async Task ColorSensorGetRawValue(MessageContainer data)
        {
            data.AddValue(nameof(colorSensor.RawValue), colorSensor.RawValue);
            await HandleOutput(data).ConfigureAwait(false);
        }

        private async Task ColorSensorGetARGB(MessageContainer data)
        {
            ARGBColor colorData = colorSensor.ColorData;
            data.AddValue(nameof(colorData.Ambient), colorData.Ambient);
            data.AddValue(nameof(colorData.Red), colorData.Red);
            data.AddValue(nameof(colorData.Green), colorData.Green);
            data.AddValue(nameof(colorData.Blue), colorData.Blue);
            await HandleOutput(data).ConfigureAwait(false);
        }
        #endregion

        #region public
        public NXTColorSensor ColorSensor { get { return this.colorSensor; } }

        #endregion
    }
}
