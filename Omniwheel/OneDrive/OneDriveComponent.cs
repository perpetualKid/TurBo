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
        OneDriveConnector oneDriveConnector;

        public OneDriveComponent() : base("OneDrive")
        {
        }

        public override async Task ComponentHelp(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELP : Shows this help screen.");
            builder.Append(Environment.NewLine);
            builder.Append("LOGIN:<ClientId>:<ClientSecret>:<AccessCode> : Logging in to OneDrive.");
            builder.Append(Environment.NewLine);
            builder.Append("LOGOUT|LOGOFF : Loging off from Onedrive.");
            builder.Append(Environment.NewLine);
            builder.Append("LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
        }

        public override async Task ProcessCommand(ControllableComponent sender, string[] commands)
        {
            switch (ResolveParameter(commands, 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(sender);
                    break;
                case "LOGIN":
                    await OneDriveLogin(sender, commands);
                    break;
                case "LOGOUT":
                case "LOGOFF":
                    await OneDriveLogout(sender, commands);
                    break;
                case "LIST":
                case "LISTFILES":
                    await OneDriveListFiles(sender, commands);
                    break;
            }
        }

        private async Task OneDriveListFiles(ControllableComponent sender, string[] commands)
        {
            string path = ResolveParameter(commands, 2);
            string filesOnlyParam = ResolveParameter(commands, 3);
            bool filesOnly = false;
            if (!bool.TryParse(filesOnlyParam, out filesOnly))
                filesOnly = (!string.IsNullOrWhiteSpace(filesOnlyParam) && filesOnlyParam.ToUpperInvariant() == "FILESONLY");

            IList<string> files = await ListFiles(path, filesOnly);
            StringBuilder builder = new StringBuilder();
            if (files != null && files.Count > 0)
            {
                foreach (string file in files)
                {
                    builder.Append(file);
                    builder.Append(Environment.NewLine);
                }
                await HandleOutput(sender, builder.ToString());
            }
            else
                await HandleOutput(sender, "No files or folder found.");
        }

        public async Task<IList<string>> ListFiles(string path, bool filesOnly)
        {
            if (null != oneDriveConnector)
            {
                var result = await oneDriveConnector.ListFilesAsync(path, filesOnly);
                return result.Value;
            }
            return null;
        }


        private async Task OneDriveLogout(ControllableComponent sender, string[] commands)
        {
            await OneDriveLogout();
            await HandleOutput(sender, "Logout " + (oneDriveConnector == null || !oneDriveConnector.LoggedIn ? "successful" : "failed"));
        }

        public async Task OneDriveLogout()
        {
            if (null != oneDriveConnector)
            await oneDriveConnector.LogoutAsync();
        }

        private async Task OneDriveLogin(ControllableComponent sender, string[] commands)
        {
            string clientId = ResolveParameter(commands, 2);
            string clientSecret = ResolveParameter(commands, 3);
            string accessCode = ResolveParameter(commands, 4);

            await OneDriveLogin(clientId, clientSecret, accessCode);
            await HandleOutput(sender, "Login " + (oneDriveConnector != null && oneDriveConnector.LoggedIn ? "successful" : "failed"));
        }

        public async Task OneDriveLogin(string clientId, string clientSecret, string accessCode)
        {
            if (null == oneDriveConnector)
                oneDriveConnector = new OneDriveConnector();
            await oneDriveConnector.LoginAsync(clientId, clientSecret, accessCode);
        }
    }
}
