using System.Threading.Tasks;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors;
using BrickPi.Uwp.Sensors.NXT;
using Devices.Components;

namespace Turbo.BrickPi.Components.Lego
{
    public class BrickPiComponent : ComponentBase
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
            await ComponentHandler.RegisterComponent(new BrickPiLedComponent("LED1", this, brickPi.Arduino1Led)).ConfigureAwait(false);
            await ComponentHandler.RegisterComponent(new BrickPiLedComponent("LED2", this, brickPi.Arduino2Led)).ConfigureAwait(false);
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
                        await ComponentHandler.RegisterComponent(new NXTColorSensorComponent("NXTColor." + sensor.SensorPort.ToString(), this, sensor as NXTColorSensor)).ConfigureAwait(false);
                        break;

                }
            }
        }

        [Action("Version")]
        [ActionHelp("Gets the current BrickPi Version. Typically used as Health Check for Communication with BrickPi.")]
        private async Task GetBrickPiVersion(MessageContainer data)
        {
            data.AddValue("Version", await brickPi.GetBrickVersion());
            await ComponentHandler.HandleOutput(data).ConfigureAwait(false);
        }

        #region Command
        #endregion

        #region public
        public Brick BrickPi { get { return this.brickPi; } }

        public int Version { get { return this.version; } }

        #endregion
    }
}
