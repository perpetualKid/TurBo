using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors.NXT;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class NXTColorSensorComponent : ComponentBase
    {
        private NXTColorSensor colorSensor;

        public NXTColorSensorComponent(string componentName, ComponentBase parent, NXTColorSensor colorSensor) : base(componentName, parent)
        {
            this.colorSensor = colorSensor;
        }

        #region command handling
        [Action("Mode")]
        [ActionHelp("Returns the current working mode for this sensor.")]
        private async Task ColorSensorGetMode(MessageContainer data)
        {
            data.AddValue("Mode", colorSensor.SensorType.ToString());
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("ColorName")]
        [ActionHelp("Returns the color name.")]
        private async Task ColorSensorGetColorName(MessageContainer data)
        {
            data.AddValue(nameof(colorSensor.ColorName), colorSensor.ColorName);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("Raw")]
        [Action("RawValue")]
        [ActionHelp(" Returns the raw sensor value.")]
        private async Task ColorSensorGetRawValue(MessageContainer data)
        {
            data.AddValue(nameof(colorSensor.RawValue), colorSensor.RawValue);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("ARGB")]
        [Action("ARGBValue")]
        [ActionHelp("Returns the Ambient Red Green Blue values.")]
        private async Task ColorSensorGetARGB(MessageContainer data)
        {
            ARGBColor colorData = colorSensor.ColorData;
            data.AddValue(nameof(colorData.Ambient), colorData.Ambient);
            data.AddValue(nameof(colorData.Red), colorData.Red);
            data.AddValue(nameof(colorData.Green), colorData.Green);
            data.AddValue(nameof(colorData.Blue), colorData.Blue);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }
        #endregion

        #region public
        public NXTColorSensor ColorSensor { get { return this.colorSensor; } }

        #endregion
    }
}
