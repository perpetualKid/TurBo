using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Sensors.NXT;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class NXTTouchSensorComponent: ComponentBase
    {
        private NXTTouchSensor touchSensor;

        public NXTTouchSensorComponent(string componentName, ComponentBase parent, NXTTouchSensor touchSensor): base(componentName, parent)
        {
            this.touchSensor = touchSensor;
        }

        [Action("Raw")]
        [Action("RawValue")]
        [ActionHelp(" Returns the raw sensor value.")]
        private async Task TouchSensorGetRawValue(MessageContainer data)
        {
            data.AddValue(nameof(touchSensor.RawValue), touchSensor.RawValue);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("Pressed")]
        [ActionHelp("Returns the boolean status whether the touch sensor is pressed.")]
        private async Task TouchSensorGetStatus(MessageContainer data)
        {
            data.AddValue(nameof(touchSensor.Pressed), touchSensor.Pressed);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }



    }
}
