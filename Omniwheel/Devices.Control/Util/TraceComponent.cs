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

        public override async Task ComponentHelp(ControllableComponent sender)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public override async Task ProcessCommand(ControllableComponent sender, string[] commands)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public static async Task Print(string sender, string text)
        {
            await HandleOutput(instance, text);
        }
    }
}
