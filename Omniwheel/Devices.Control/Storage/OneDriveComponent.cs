using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Devices.Base;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Devices.Control.Storage
{
    public class OneDriveControllable : Controllable
    {
        OneDriveConnector oneDriveConnector;

        public OneDriveControllable() : base("OneDrive")
        {
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.Responses.Add("HELP : Shows this help screen.");
            data.Responses.Add("LOGIN:<ClientId>:<ClientSecret>:<AccessCode> : Logging in to OneDrive.");
            data.Responses.Add("LOGOUT|LOGOFF : Loging off from Onedrive.");
            data.Responses.Add("LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            await HandleOutput(data);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data);
                    break;
                case "LOGIN":
                    await OneDriveLogin(data);
                    break;
                case "LOGOUT":
                case "LOGOFF":
                    await OneDriveLogout(data);
                    break;
                case "LIST":
                case "LISTFILES":
                    await OneDriveListFiles(data);
                    break;
            }
        }

        #region private implementation
        private async Task OneDriveListFiles(MessageContainer data)
        {
            string path = ResolveParameter(data, 2);
            string filesOnlyParam = ResolveParameter(data, 3);
            bool filesOnly = false;
            if (!bool.TryParse(filesOnlyParam, out filesOnly))
                filesOnly = (!string.IsNullOrWhiteSpace(filesOnlyParam) && filesOnlyParam.ToUpperInvariant() == "FILESONLY");

            IList<string> files = await ListFiles(path, filesOnly);
            StringBuilder builder = new StringBuilder();
            if (files != null && files.Count > 0)
            {
                foreach (string file in files)
                {
                    data.Responses.Add(file);
                }
            }
            else
                data.Responses.Add("No files or folder found.");
            await HandleOutput(data); ;
        }

        private async Task OneDriveLogin(MessageContainer data)
        {
            string clientId = ResolveParameter(data, 2);
            string clientSecret = ResolveParameter(data, 3);
            string accessCode = ResolveParameter(data, 4);

            await OneDriveLogin(clientId, clientSecret, accessCode);
            data.Responses.Add("Login " + (oneDriveConnector != null && oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await HandleOutput(data); ;
        }

        private async Task OneDriveLogout(MessageContainer data)
        {
            await OneDriveLogout();
            data.Responses.Add("Logout " + (oneDriveConnector == null || !oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await HandleOutput(data);;
        }
        #endregion

        #region public
        public async Task<IList<string>> ListFiles(string path, bool filesOnly)
        {
            if (null != oneDriveConnector)
            {
                var result = await oneDriveConnector.ListFilesAsync(path, filesOnly);
                return result.Value;
            }
            return null;
        }

        public async Task OneDriveLogout()
        {
            if (null != oneDriveConnector)
            await oneDriveConnector.LogoutAsync();
        }

        public async Task OneDriveLogin(string clientId, string clientSecret, string accessCode)
        {
            if (null == oneDriveConnector)
            {
                oneDriveConnector = new OneDriveConnector();
                oneDriveConnector.TokensChangedEvent += OneDriveConnector_TokensChangedEvent;
            }
            await oneDriveConnector.LoginAsync(clientId, clientSecret, accessCode);
        }

        private void OneDriveConnector_TokensChangedEvent(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public async Task UploadFile(StorageFile file, string path)
        {
            if (null != oneDriveConnector)
            {
                await oneDriveConnector.UploadFileAsync(file, path);
            }
        }

        public async Task UploadFile(IInputStream stream, string path, string fileName)
        {
            if (null != oneDriveConnector)
            {
                await oneDriveConnector.UploadFileAsync(stream, path, fileName);
            }
        }
        #endregion
    }
}
