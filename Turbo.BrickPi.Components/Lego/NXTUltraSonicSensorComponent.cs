using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Sensors.NXT;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class NXTUltraSonicSensorComponent: ComponentBase
    {

        private NXTUltraSonicSensor ultrasonicSensor;

        public NXTUltraSonicSensorComponent(string componentName, ComponentBase parent, NXTUltraSonicSensor ultrasonicSensor): base(componentName, parent)
        {
            this.ultrasonicSensor = ultrasonicSensor;
        }

        [Action("Raw")]
        [Action("RawValue")]
        [ActionHelp(" Returns the raw sensor value.")]
        private async Task UltrasonicSensorGetRawValue(MessageContainer data)
        {
            data.AddValue(nameof(ultrasonicSensor.RawValue), ultrasonicSensor.RawValue);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("Distance")]
        [ActionHelp("Returns the Distance measured by the Ultrasonic sensor.")]
        private async Task UltrasonicSensorGetDistance(MessageContainer data)
        {
            data.AddValue(nameof(ultrasonicSensor.Distance), ultrasonicSensor.Distance);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        #region public
        public NXTUltraSonicSensor UltraSonicSensor { get { return this.ultrasonicSensor; } }
        #endregion
    }
}
