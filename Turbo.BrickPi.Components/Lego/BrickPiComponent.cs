using System.Threading.Tasks;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors;
using BrickPi.Uwp.Sensors.NXT;
using Devices.Controllable;

namespace Turbo.BrickPi.Components.Lego
{
    public class BrickPiComponent : ControllableComponent
    {
        private Brick brickPi;
        private int version;

        public BrickPiComponent() : base("BrickPi")
        {

        }

        protected override async Task InitializeDefaults()
        {
            brickPi = await Brick.InitializeInstance("Uart0").ConfigureAwait(false);
            version = await brickPi.GetBrickVersion().ConfigureAwait(false);
            await RegisterComponent(new BrickPiLedComponent("LED1", this, brickPi.Arduino1Led)).ConfigureAwait(false);
            await RegisterComponent(new BrickPiLedComponent("LED2", this, brickPi.Arduino2Led)).ConfigureAwait(false);
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
                        await RegisterComponent(new NXTColorSensorComponent("NXTColor." + sensor.SensorPort.ToString(), this, sensor as NXTColorSensor)).ConfigureAwait(false);
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
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (data.ResolveParameter(nameof(MessageContainer.FixedPropertyNames.Action), 1).ToUpperInvariant())
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

        public int Version { get { return this.version; } }

        #endregion
    }
}
