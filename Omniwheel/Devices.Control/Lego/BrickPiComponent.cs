using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp;
using Common.Base;

namespace Devices.Control.Lego
{
    public class BrickPiComponent : Controllable
    {
        private Brick brickPi;

        public BrickPiComponent() : base("BrickPi")
        {

        }

        protected override async Task InitializeDefaults()
        {
            brickPi = await Brick.InitializeInstance("Uart0");
            await RegisterComponent(new BrickPiLedComponent("LED1", this, brickPi.Arduino1Led));
            await RegisterComponent(new BrickPiLedComponent("LED2", this, brickPi.Arduino2Led));
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.Responses.Add("HELP : Shows this help screen.");
            data.Responses.Add("VERSION : Gets the current BrickPi Version. Often used as Health Check.");
            data.Responses.Add("CONNECT:<StorageConnectionString>|<StorageAccount>:<AccessKey> : Connecting to Azure Blob Storage.");
            data.Responses.Add("DISCONNECT : Disconnecting from Azure Blob Storage.");
            data.Responses.Add("LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            await HandleOutput(data);
        }

        protected async Task BrickPiVersion(MessageContainer data)
        {
            data.Responses.Add($"BrickPi Version {await brickPi.GetBrickVersion()}");
            await HandleOutput(data);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data);
                    break;
                case "VERSION":
                    await BrickPiVersion(data);
                    break;
                case "DISCONNECT":
                    break;
                case "LIST":
                case "LISTFILES":
                    break;
            }
        }

        #region Command
        private async Task BrickPiSetArduino1Led(MessageContainer data)
        {
            string path = ResolveParameter(data, 2);
            string filesOnlyParam = ResolveParameter(data, 3);
            bool filesOnly = false;
            if (!bool.TryParse(filesOnlyParam, out filesOnly))
                filesOnly = (!string.IsNullOrWhiteSpace(filesOnlyParam) && filesOnlyParam.ToUpperInvariant() == "FILESONLY");

            data.Responses.Add(await ListComponents().ConfigureAwait(false));
            await HandleOutput(data).ConfigureAwait(false);
        }
        #endregion

        #region public
        public Brick BrickPi { get { return this.brickPi; } }

        public async Task SetArduinoLed1(bool light)
        {
            brickPi.Arduino1Led.Light = light;
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task SetArduinoLed2(bool light)
        {
            brickPi.Arduino2Led.Light = light;
            await Task.CompletedTask.ConfigureAwait(false);
        }
        #endregion
    }
}
