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

        protected override async Task ComponentHelp(MessageContainer data)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            throw new NotImplementedException();
        }


        protected override async Task ProcessCommand(MessageContainer data)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public static async Task Print(string sender, string text)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            throw new NotImplementedException();
        }
    }
}
