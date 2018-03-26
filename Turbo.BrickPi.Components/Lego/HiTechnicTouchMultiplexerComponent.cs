using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Sensors.Hitechnic;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class HiTechnicTouchMultiplexerComponent: ComponentBase
    {
        private HiTechnicTouchMultiplexer touchMultiplexer;

        public HiTechnicTouchMultiplexerComponent(string componentName, ComponentBase parent, HiTechnicTouchMultiplexer touchMultiplexer) : base(componentName, parent)
        {
            this.touchMultiplexer = touchMultiplexer;
        }

        [Action("Raw")]
        [Action("RawValue")]
        [ActionHelp(" Returns the raw multiplexer value.")]
        private async Task TouchMultiplexerGetRawValue(MessageContainer data)
        {
            data.AddValue(nameof(touchMultiplexer.RawValue), touchMultiplexer.RawValue);
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }


    }
}
