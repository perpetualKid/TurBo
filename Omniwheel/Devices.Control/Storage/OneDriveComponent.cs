using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Common.Base.Categories;
using Devices.Base;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Devices.Control.Storage
{
    public class OneDriveComponent : StorageControllable
    {
        OneDriveConnector oneDriveConnector;

        public OneDriveComponent() : base("OneDrive")
        {
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "LOGIN:<ClientId>:<ClientSecret>:<AccessCode> : Logging in to OneDrive.");
            data.AddMultiPartValue("Help", "LOGOUT|LOGOFF : Loging off from Onedrive.");
            data.AddMultiPartValue("Help", "LIST|LISTFILES[:<Path>[:<FilesOnly|True>]] : List Folders and Files or Files only.");
            await HandleOutput(data);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (ResolveParameter(data, "Action", 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data);
                    break;
                case "LOGIN":
                case "LOGON":
                    await ConnectStorage(data);
                    break;
                case "LOGOUT":
                case "LOGOFF":
                    await DisconnectStorage(data);
                    break;
                case "LIST":
                case "LISTFILES":
                    await ListContent(data);
                    break;
            }
        }

        #region private implementation
        protected override async Task ListContent(MessageContainer data)
        {
            string path = ResolveParameter(data, "Path", 2);
            string filesOnlyParam = ResolveParameter(data, "FilesOnly", 3);
            bool filesOnly = false;
            if (!bool.TryParse(filesOnlyParam, out filesOnly))
                filesOnly = (!string.IsNullOrWhiteSpace(filesOnlyParam) && filesOnlyParam.ToUpperInvariant() == "FILESONLY");

            IList<string> files = await ListFiles(path, filesOnly);
            StringBuilder builder = new StringBuilder();
            if (files != null && files.Count > 0)
            {
                data.AddValue("Files", files);
            }
            else
                data.AddValue("EmptyFiles", "No files or folder found.");
            await HandleOutput(data); ;
        }

        protected override async Task ConnectStorage(MessageContainer data)
        {
            string clientId = ResolveParameter(data, "ClientId", 2);
            string clientSecret = ResolveParameter(data, "ClientSecret", 3);
            string accessCode = ResolveParameter(data, "AccessCode", 4);

            await OneDriveLogin(clientId, clientSecret, accessCode);
            data.AddValue("Login", "Login " + (oneDriveConnector != null && oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await HandleOutput(data); ;
        }

        protected override async Task DisconnectStorage(MessageContainer data)
        {
            await OneDriveLogout();
            data.AddValue("Logout", "Logout " + (oneDriveConnector == null || !oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await HandleOutput(data); ;
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
