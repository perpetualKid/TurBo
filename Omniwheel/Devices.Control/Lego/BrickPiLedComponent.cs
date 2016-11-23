using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using Common.Base;

namespace Devices.Control.Lego
{
    public class BrickPiLedComponent : Controllable
    {
        private BrickLed brickLed;

        public BrickPiLedComponent(string componentName, Controllable parent, BrickLed led) : base(componentName, parent)
        {
            this.brickLed = led;
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.Responses.Add("LED:HELP : Shows this help screen.");
            data.Responses.Add("LED:ON|ENABLED : Turns the LED on.");
            data.Responses.Add("LED:OFF|DISABLED : Turns the LED off.");
            data.Responses.Add("LED:TOGGLE : Toggle the LED from current status.");
            data.Responses.Add("LED:STATUS : Returns the current status for the LED.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data).ConfigureAwait(false);
                    break;
                case "ON":
                case "ENABLED":
                    await SetLed(true).ConfigureAwait(false);
                    break;
                case "OFF":
                case "DISABLED":
                    await SetLed(false).ConfigureAwait(false);
                    break;
                case "TOGGLE":
                    await ToogleLed().ConfigureAwait(false);
                    break;
                case "STATUS":
                    await LedComponentGetStatus(data).ConfigureAwait(false);
                    break;
            }
        }

        #region command handling
        private async Task LedComponentGetStatus(MessageContainer data)
        {
            data.Responses.Add(await GetLedStatus().ConfigureAwait(false) ? "Enabled" : "Disabled");
            await HandleOutput(data).ConfigureAwait(false);
        }
        #endregion

        #region public
        public async Task<bool> GetLedStatus()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return brickLed.Light;
        }

        public async Task ToogleLed()
        {
            brickLed.Toggle();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task SetLed(bool status)
        {
            brickLed.Light = status;
            await Task.CompletedTask.ConfigureAwait(false);
        }
        #endregion
    }
}
