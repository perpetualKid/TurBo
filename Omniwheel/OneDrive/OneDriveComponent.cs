using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devices.Control.Base;

namespace OneDrive
{
    public class OneDriveComponent : ControllableComponent
    {
        public OneDriveComponent() : base("OneDrive")
        {
        }

        public override async Task ComponentHelp(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELP ::  Shows this help screen");
            builder.Append(Environment.NewLine);
            builder.Append("LOGIN :: LOGIN to OneDrive");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
        }

        public override async Task ProcessCommand(ControllableComponent sender, string[] commands)
        {
//            string param;
            switch (ResolveParameter(commands, 1))
            {
                case "HELP":
                    await ComponentHelp(sender);
                    break;
            }
        }
    }
}
