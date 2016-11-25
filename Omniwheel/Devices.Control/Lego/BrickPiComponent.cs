using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors;
using BrickPi.Uwp.Sensors.NXT;
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

        public async Task RegisterSensors()
        {
            foreach (RawSensor sensor in brickPi.Sensors)
            {
                switch (sensor.SensorType)
                {   
                    case SensorType.COLOR_NONE:
                    case SensorType.COLOR_BLUE:
                    case SensorType.COLOR_GREEN:
                    case SensorType.COLOR_RED:
                    case SensorType.COLOR_FULL:
                        await RegisterComponent(new NXTColorSensorComponent("NXTColor." + sensor.SensorPort.ToString(), this, sensor as NXTColorSensor));
                        break;

                }
            }
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

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data).ConfigureAwait(false);
                    break;
                case "VERSION":
                    await GetBrickPiVersion(data).ConfigureAwait(false);
                    break;
                case "DISCONNECT":
                    break;
                case "LIST":
                case "LISTFILES":
                    break;
            }
        }

        private async Task GetBrickPiVersion(MessageContainer data)
        {
            data.AddValue("Version", await brickPi.GetBrickVersion());
            await HandleOutput(data).ConfigureAwait(false);
        }

        #region Command
        #endregion

        #region public
        public Brick BrickPi { get { return this.brickPi; } }

        #endregion
    }
}
