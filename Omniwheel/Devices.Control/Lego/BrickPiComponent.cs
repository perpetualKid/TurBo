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
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.Responses.Add("HELP : Shows this help screen.");
            data.Responses.Add("VERSION : Gets the current BrickPi Version. Often used as Health Check.");
            data.Responses.Add("CONNECT:<StorageConnectionString>|<StorageAccount>:<AccessKey> : Connecting to Azure Blob Storage.");
            data.Responses.Add("DISCONNECT: Disconnecting from Azure Blob Storage.");
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

        #region public
        public Brick BrickPi { get { return this.brickPi; } }

        #endregion
    }
}
