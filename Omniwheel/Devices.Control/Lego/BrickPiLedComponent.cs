using System.Threading.Tasks;
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
            data.AddMultiPartValue("Help", "LED HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "LED ON|ENABLE : Turns the LED on.");
            data.AddMultiPartValue("Help", "LED OFF|DISABLE : Turns the LED off.");
            data.AddMultiPartValue("Help", "LED TOGGLE : Toggle the LED from current status.");
            data.AddMultiPartValue("Help", "LED STATUS : Returns the current status for the LED.");
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
                case "ENABLE":
                    await SetLed(true).ConfigureAwait(false);
                    break;
                case "OFF":
                case "DISABLE":
                    await SetLed(false).ConfigureAwait(false);
                    break;
                case "TOGGLE":
                    await LedComponentToogle(data).ConfigureAwait(false);
                    break;
                case "STATUS":
                    await LedComponentGetStatus(data).ConfigureAwait(false);
                    break;
            }
        }

        #region command handling
        private async Task LedComponentGetStatus(MessageContainer data)
        {
            data.AddValue("Status", (await GetLedStatus().ConfigureAwait(false) ? "Enabled" : "Disabled"));
            await HandleOutput(data).ConfigureAwait(false);
        }

        private async Task LedComponentToogle(MessageContainer data)
        {
            await ToogleLed().ConfigureAwait(false);
            await LedComponentGetStatus(data).ConfigureAwait(false);
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
