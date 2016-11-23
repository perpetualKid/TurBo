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
            data.AddMultiPartValue("Help", "HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "VERSION : Gets the current BrickPi Version. Often used as Health Check.");
            data.AddMultiPartValue("Help", "CONNECT:<StorageConnectionString>|<StorageAccount>:<AccessKey> : Connecting to Azure Blob Storage.");
            data.AddMultiPartValue("Help", "DISCONNECT : Disconnecting from Azure Blob Storage.");
            data.AddMultiPartValue("Help", "LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            await HandleOutput(data);
        }

        protected async Task BrickPiVersion(MessageContainer data)
        {
            data.AddValue("Version", $"BrickPi Version {await brickPi.GetBrickVersion()}");
            await HandleOutput(data);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
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
        #endregion

        #region public
        public Brick BrickPi { get { return this.brickPi; } }

        #endregion
    }
}
