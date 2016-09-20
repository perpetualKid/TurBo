using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devices.Control.Base;

namespace Devices.Control.Util
{
    public class TraceComponent : ControllableComponent
    {
        private static TraceComponent instance;

        static TraceComponent()
        {
            instance = new TraceComponent();
        }

        private TraceComponent(): base("TRACE")
        {

        }

        public override void ComponentHelp()
        {
            throw new NotImplementedException();
        }

        public override void ProcessCommand(ControllableComponent sender, string[] commands)
        {
            throw new NotImplementedException();
        }

        public static async Task Print(string sender, string text)
        {
            HandleOutput(instance, text);
        }
    }
}
