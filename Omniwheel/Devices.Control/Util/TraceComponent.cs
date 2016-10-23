using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Base;

namespace Devices.Control.Util
{
    public class TraceComponent : Controllable
    {
        private static TraceComponent instance;

        static TraceComponent()
        {
            instance = new TraceComponent();
        }

        private TraceComponent(): base("TRACE")
        {

        }

        public override async Task ComponentHelp(MessageContainer data)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }


        public override async Task ProcessCommand(MessageContainer data)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public static async Task Print(string sender, string text)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
