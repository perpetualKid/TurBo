using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class BrickPiLedComponent : ComponentBase
    {
        private BrickLed brickLed;

        public BrickPiLedComponent(string componentName, ComponentBase parent, BrickLed led) : base(componentName, parent)
        {
            this.brickLed = led;
        }


        #region command handling
        [Action("Status")]
        [ActionHelp("Returns the current status for the LED.")]
        private async Task LedComponentGetStatus(MessageContainer data)
        {
            data.AddValue("Status", (await GetLedStatus().ConfigureAwait(false) ? "Enabled" : "Disabled"));
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        [Action("Toggle")]
        [ActionHelp("Toggle the LED from current status.")]
        [ActionParameter("SendStatus", ParameterType = typeof(bool), Required = false)]
        private async Task LedComponentToogle(MessageContainer data)
        {
            if (!bool.TryParse(data.ResolveParameter("SendStatus", 0), out bool param))
                param = false;

            await ToogleLed().ConfigureAwait(false);
            if (param)
                await LedComponentGetStatus(data).ConfigureAwait(false);
        }

        [Action("Enable")]
        [Action("On")]
        [ActionHelp("Turns the LED on.")]
        private async Task EnableLed(MessageContainer data)
        {
            await SetLed(true);
        }

        [Action("Disable")]
        [Action("Off")]
        [ActionHelp("Turns the LED off.")]
        private async Task DisableLed(MessageContainer data)
        {
            await SetLed(false);
        }
        #endregion

        #region public
        public Task<bool> GetLedStatus()
        {
            return Task.FromResult<bool>(brickLed.Light);
        }

        public Task ToogleLed()
        {
            brickLed.Toggle();
            return Task.CompletedTask;
        }

        public Task SetLed(bool status)
        {
            brickLed.Light = status;
            return Task.CompletedTask;
        }
        #endregion
    }
}
