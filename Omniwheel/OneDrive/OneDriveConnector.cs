using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;



namespace OneDrive
{
    public sealed class OneDriveConnector
    {
        private const string UploadUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}/{1}:/content";
        private const string DeleteUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}/{1}";
        private const string ListUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}:/children";
        private const string TokenUri = "https://login.live.com/oauth20_token.srf";
        private const string TokenContentFormatAccess = "client_id={0}&redirect_uri={1}&{2}={3}&grant_type={4}";

        private const string OneDriveRedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private const string OneDriveLoginUrlFormat = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=code&redirect_uri={2}";
        private const string OneDriveLogoutUrlFormat = "https://login.live.com/oauth20_logout.srf?client_id={0}&redirect_uri={1}";
        private const string OneDriveScope = "wl.offline_access onedrive.readwrite";

        private HttpClient httpClient;
        private Timer refreshTimer;

        private string clientId;
        private string clientSecret;
        private string redirectUrl;

        public event EventHandler TokensChangedEvent;

        public bool LoggedIn { get; private set; } = false;

        public string AccessToken { get; private set; } = string.Empty;

        public string RefreshToken { get; private set; } = string.Empty;


        /// <summary>
        /// Instantiates a OneDrive connector object. Requires a call to "login" function to complete authorization.
        /// </summary>
        public OneDriveConnector()
        {
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            filter.AllowUI = false;
            httpClient = new HttpClient(filter);
        }

        /// <summary>
        /// Obtains authorization codes from OneDrive login service. Requires access code to be obtained from OneDrive as described in the OneDrive authorization documentation.
        /// </summary>
        /// <param name="clientId"></param> Client ID obtained from app registration
        /// <param name="clientSecret"></param> Client secret obtained from app registration
        /// <param name="accessCode"></param> Access Code obtained from earlier login prompt.
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> LoginAsync(string clientId, string clientSecret, string accessCode)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUrl = OneDriveRedirectUrl;

            return Task.Run(async () =>
            {
                HttpResponseMessage response = await GetTokens(accessCode, "code", "authorization_code");
                StartTimer();
                return response;
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Reauthorizes the connection to OneDrive with the provided access and refresh tokens, and saves those tokens internally for future use
        /// </summary>
        /// <param name="clientId"></param> Client ID obtained from app registration
        /// <param name="clientSecret"></param> Client secret obtained from app registration
        /// <param name="redirectUrl"></param> Redirect URL obtained from app registration
        /// <param name="refreshToken"></param>
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> Reauthorize(string clientIdIn, string clientSecretIn, string refreshTokenIn)
        {
            clientId = clientIdIn;
            clientSecret = clientSecretIn;
            redirectUrl = OneDriveRedirectUrl;

            return Task.Run(async () =>
            {
                HttpResponseMessage response = await GetTokens(refreshTokenIn, "refresh_token", "refresh_token");
                StartTimer();
                return response;
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Calls the OneDrive reauth service with current authorization tokens
        /// </summary>
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> Reauthorize()
        {
            return Task.Run(async () =>
            {
                return await Reauthorize(clientId, clientSecret, RefreshToken);
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Uploads a file to OneDrive. This method is NOT thread safe. It assumes that the contents of the file will not change during the upload process. 
        /// </summary>
        /// <param name="file"></param> The file to upload to OneDrive. The file will be read, and a copy uploaded. The original file object will not be modified.
        /// <param name="destinationPath"></param> The path to the destination on Onedrive. Passing in an empty string will place the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> UploadFileAsync(StorageFile file, string path)
        {
            string uploadUri = String.Format(UploadUrlFormat, CorrectOneDrivePath(path), file.Name);

            return Task.Run(async () =>
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (HttpStreamContent streamContent = new HttpStreamContent(stream.AsInputStream()))
                    {
                        using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, new Uri(uploadUri)))
                        {
                            requestMessage.Content = streamContent;

                            using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                            {
                                return response;
                            }
                        }
                    }
                }
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Stores stream content as file on OneDrive. This method is NOT thread safe. It assumes that the contents of the stream will not change during the upload process. 
        /// </summary>
        /// <param name="file"></param> The stream to upload to OneDrive. 
        /// <param name="destinationPath"></param> The path to the destination on Onedrive. Passing in an empty string will place the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> UploadFileAsync(IInputStream stream, string path, string fileName)
        {
            string uploadUri = String.Format(UploadUrlFormat, CorrectOneDrivePath(path), fileName);

            return Task.Run(async () =>
            {
                using (HttpStreamContent streamContent = new HttpStreamContent(stream))
                {
                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, new Uri(uploadUri)))
                    {
                        requestMessage.Content = streamContent;

                        using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                        {
                            return response;
                        }
                    }
                }
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Deletes a file to OneDrive.
        /// </summary>
        /// <param name="fileName"></param> The name of the file to delete
        /// <param name="pathToFile"></param> The path to the file on Onedrive. Passing in an empty string will look for the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> DeleteFileAsync(string fileName, string path)
        {
            string deleteUri = String.Format(DeleteUrlFormat, CorrectOneDrivePath(path), fileName);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, new Uri(deleteUri)))
            {
                return Task.Run(async () =>
                {
                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                    {
                        return response;
                    }
                }).AsAsyncOperation<HttpResponseMessage>();
            }
        }

        /// <summary>
        /// List the names of all the files and folders in a OneDrive folder.
        /// </summary>
        /// <param name="folderPath"></param> The path to the folder on OneDrive. Passing in an empty string will list the files in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random".
        /// <returns>A key-value pair containing the response message given by the server for the request as the key and a list containing the names of the files as the value</returns>
        public IAsyncOperation<KeyValuePair<HttpResponseMessage, IList<string>>> ListFilesAsync(string path, bool filesOnly = false)
        {
            return Task.Run(async () =>
            {
                string listUri = String.Format(ListUrlFormat, CorrectOneDrivePath(path));

                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(listUri)))
                {
                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                    {
                        IList<string> files = new List<string>();
                        using (var inputStream = await response.Content.ReadAsInputStreamAsync())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (Stream testStream = inputStream.AsStreamForRead())
                                {
                                    using (StreamReader reader = new StreamReader(inputStream.AsStreamForRead()))
                                    {
                                        //Get file and folder names
                                        string result = await reader.ReadToEndAsync();
                                        JsonObject json;
                                        if (JsonObject.TryParse(result, out json) && json.ContainsKey("value"))
                                        {
                                            foreach (var item in json["value"].GetArray())
                                            {
                                                if (filesOnly && item.GetObject().ContainsKey("folder"))
                                                    continue;
                                                files.Add(item.GetObject().GetNamedString("name"));
                                            }
                                        }
                                        return new KeyValuePair<HttpResponseMessage, IList<string>>(response, files);
                                    }
                                }
                            }
                        }
                    }
                }
            }).AsAsyncOperation<KeyValuePair<HttpResponseMessage, IList<string>>>();
        }

        /// <summary>
        /// Disposes of any user specific data obtained during login process.
        /// </summary>
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> LogoutAsync()
        {
            clientId = string.Empty;
            clientSecret = string.Empty;
            redirectUrl = string.Empty;
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            if (null != refreshTimer)
                refreshTimer.Dispose();
            if (null != httpClient)
            {
                httpClient.DefaultRequestHeaders.Clear();
                //httpClient.Dispose();
            }
            LoggedIn = false;

            EventHandler handler = TokensChangedEvent;

            if (null != handler)
            {
                handler(this, new EventArgs());
            }

            string logoutUri = string.Format(OneDriveLogoutUrlFormat, clientId, redirectUrl);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(logoutUri)))
            {
                return Task.Run(async () =>
                {
                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                    {
                        return response;
                    }
                }).AsAsyncOperation<HttpResponseMessage>();
            }
        }

        /// <summary>
        /// Generates the url for the OneDrive login page
        /// </summary>
        /// <returns></returns>
        public static string GenerateOneDriveLoginUrl(string clientId)
        {
            // Create OneDrive URL for logging in
            return string.Format(OneDriveLoginUrlFormat, clientId, OneDriveScope, OneDriveRedirectUrl);
        }

        /// <summary>
        /// Generates the html for the OneDrive login page
        /// </summary>
        /// <returns></returns>
        public static string GenerateOneDriveLoginPage(string clientId)
        {
            StringBuilder htmlString = new StringBuilder();
            htmlString.Append("<p class='sectionHeader'>Log into OneDrive:</p>");
            htmlString.Append("<ol>");
            htmlString.Append("<li>Click on this link:  <a href='");
            htmlString.Append(GenerateOneDriveLoginUrl(clientId));
            htmlString.Append("' target='_blank'>OneDrive Login</a><br>");
            htmlString.Append("A new window will open.  Log into OneDrive.<br><br></li>");
            htmlString.Append("<li>After you're done, you should arrive at a blank page.<br>");
            htmlString.Append("Copy the URL of the landing page and return to me.<br>");
            htmlString.Append("The URL will look something like this: https://login.live.com/oauth20_desktop.srf?code=M6b0ce71e-8961-1395-2435-f78db54f82ae&lc=1033 <br>");
            htmlString.Append("</ol><br><br>");

            return htmlString.ToString();
        }

        public static string ParseAccessCode(Uri uri)
        {
            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(uri.Query);
            try
            {
                return decoder.GetFirstValueByName("code");
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static string ParseAccessCode(string uriString)
        {
            return ParseAccessCode(new Uri(uriString));
        }


        private async void ReauthorizeOnTimer(object stateInfo)
        {
            await Reauthorize();
        }

        private void StartTimer()
        {
            TimerCallback callBack = this.ReauthorizeOnTimer;
            //accessToken expires after one hour by default
            TimeSpan dueTime = new TimeSpan(0, 50, 0);
            if (null == refreshTimer)
                refreshTimer = new Timer(callBack, null, dueTime, Timeout.InfiniteTimeSpan);
            else
                refreshTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
        }

        private async Task<HttpResponseMessage> GetTokens(string accessCodeOrRefreshToken, string requestType, string grantType)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(TokenUri)))
            {
                string requestContent = string.Format(TokenContentFormatAccess, clientId, redirectUrl, requestType, accessCodeOrRefreshToken, grantType);
                requestMessage.Content = new HttpStringContent(requestContent);
                requestMessage.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                using (HttpResponseMessage responseMessage = await httpClient.SendRequestAsync(requestMessage))
                {
                    string responseContentString = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        JsonObject json;
                        if (JsonObject.TryParse(responseContentString, out json))
                        {
                            AccessToken = json.GetNamedString("access_token");
                            RefreshToken = json.GetNamedString("refresh_token");
                        }

                        httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", AccessToken);
                        TokensChangedEvent?.Invoke(this, new EventArgs());

                        LoggedIn = true;
                    }
                    return responseMessage;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param> Passing in an empty string will place the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns></returns>
        private static string CorrectOneDrivePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                return string.Empty;
            return path.StartsWith("/") ? path : "/" + path;
        }
    }
}
