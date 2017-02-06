using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Devices.Components;
using Devices.Connectors.Storage;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Turbo.BrickPi.Components.Storage
{
    public class OneDriveComponent : StorageComponentBase
    {
        OneDriveConnector oneDriveConnector;

        public OneDriveComponent() : base("OneDrive")
        {
        }

        #region private implementation
        [Action("List")]
        [Action("ListFiles")]
        [ActionParameter("Path", Required = false)]
        [ActionParameter("FilesOnly", ParameterType = typeof(bool), Required = false)]
        [ActionHelp("List Folders and Files, starting at root or in given path. Set FilesOnly to list only files.")]
        protected override async Task ListContent(MessageContainer data)
        {
            string path = data.ResolveParameter("Path", 2);
            string filesOnlyParam = data.ResolveParameter("FilesOnly", 3);
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
            await ComponentHandler.HandleOutput(data); ;
        }

        [Action("Login")]
        [Action("Logon")]
        [Action("Connect")]
        [ActionParameter("ClientId")]
        [ActionParameter("ClientSecret")]
        [ActionParameter("AccessCode")]
        [ActionHelp("Logging in to OneDrive.")]
        protected override async Task ConnectStorage(MessageContainer data)
        {
            string clientId = data.ResolveParameter("ClientId", 2);
            string clientSecret = data.ResolveParameter("ClientSecret", 3);
            string accessCode = data.ResolveParameter("AccessCode", 4);

            await OneDriveLogin(clientId, clientSecret, accessCode);
            data.AddValue("Login", "Login " + (oneDriveConnector != null && oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await ComponentHandler.HandleOutput(data); ;
        }

        [Action("Logout")]
        [Action("Logoff")]
        [Action("Disconnect")]
        [ActionHelp("Loging off from Onedrive.")]
        protected override async Task DisconnectStorage(MessageContainer data)
        {
            await OneDriveLogout();
            data.AddValue("Logout", "Logout " + (oneDriveConnector == null || !oneDriveConnector.LoggedIn ? "successful" : "failed"));
            await ComponentHandler.HandleOutput(data); ;
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
