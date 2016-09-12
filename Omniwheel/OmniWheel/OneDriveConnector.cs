﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;



namespace OmniWheel
{
    public sealed class OneDriveConnector
    {
        /// <summary>
        /// Is true if currently logged in to OneDrive, false otherwise.
        /// </summary>
        public bool isLoggedIn { get; private set; } = false;
        public string accessToken { get; private set; } = string.Empty;
        public string refreshToken { get; private set; } = string.Empty;

        private const string LoginUriFormat = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope=wl.offline_access onedrive.readwrite&response_type=code&redirect_uri={1}";
        private const string LogoutUriFormat = "https://login.live.com/oauth20_logout.srf?client_id={0}&redirect_uri={1}";
        private const string UploadUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}/{1}:/content";
        private const string DeleteUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}/{1}";
        private const string ListUrlFormat = "https://api.onedrive.com/v1.0/drive/root:{0}:/children";
        private const string TokenUri = "https://login.live.com/oauth20_token.srf";
        private const string TokenContentFormatAccess = "client_id={0}&redirect_uri={1}&{2}={3}&grant_type={4}";


        private HttpClient httpClient;
        private Timer refreshTimer;
        private string clientId = string.Empty;
        private string clientSecret = string.Empty;
        private string redirectUrl = string.Empty;

        public event EventHandler<string> TokensChangedEvent;

        /// <summary>
        /// Instantiates a OneDrive connector object. Requires a call to "login" function to complete authorization.
        /// </summary>
        public OneDriveConnector()
        {
            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            filter.AllowUI = false;
            httpClient = new HttpClient(filter);
        }

        public string FormatAccessTokenUriString(string clientId, string redirectUri)
        {
            return string.Format(LoginUriFormat, clientId, redirectUri);
        }

        /// <summary>
        /// Obtains authorization codes from OneDrive login service. Requires access code to be obtained from OneDrive as described in the OneDrive authorization documentation.
        /// </summary>
        /// <param name="clientId"></param> Client ID obtained from app registration
        /// <param name="clientSecret"></param> Client secret obtained from app registration
        /// <param name="redirectUrl"></param> Redirect URL obtained from app registration
        /// <param name="accessCode"></param> Access Code obtained from earlier login prompt.
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> LoginAsync(string clientId, string clientSecret, string redirectUrl, string accessCode)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUrl = redirectUrl;

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
        public IAsyncOperation<HttpResponseMessage> Reauthorize(string clientIdIn, string clientSecretIn, string redirectUrlIn, string refreshTokenIn)
        {
            clientId = clientIdIn;
            clientSecret = clientSecretIn;
            redirectUrl = redirectUrlIn;

            return Task.Run(async () =>
            {
                HttpResponseMessage response = await GetTokens(refreshTokenIn, "refresh_token", "refresh_token");
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
                HttpResponseMessage response = await Reauthorize(clientId, clientSecret, redirectUrl, refreshToken);
                return response;
            }).AsAsyncOperation<HttpResponseMessage>();
        }

        /// <summary>
        /// Uploads a file to OneDrive. This method is NOT thread safe. It assumes that the contents of the file will not change during the upload process. 
        /// </summary>
        /// <param name="file"></param> The file to upload to OneDrive. The file will be read, and a copy uploaded. The original file object will not be modified.
        /// <param name="destinationPath"></param> The path to the destination on Onedrive. Passing in an empty string will place the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> UploadFileAsync(StorageFile file, string destinationPath)
        {
            string uploadUri = String.Format(UploadUrlFormat, destinationPath, file.Name);

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
        /// Deletes a file to OneDrive.
        /// </summary>
        /// <param name="fileName"></param> The name of the file to delete
        /// <param name="pathToFile"></param> The path to the file on Onedrive. Passing in an empty string will look for the file in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random"
        /// <returns>The response message given by the server for the request</returns>
        public IAsyncOperation<HttpResponseMessage> DeleteFileAsync(string fileName, string pathToFile)
        {
            string deleteUri = String.Format(DeleteUrlFormat, pathToFile, fileName);
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
        /// List the names of all the files in a OneDrive folder.
        /// </summary>
        /// <param name="folderPath"></param> The path to the folder on OneDrive. Passing in an empty string will list the files in the root of Onedrive. Other folder paths should be passed in with a leading '/' character, such as "/Documents" or "/Pictures/Random".
        /// <returns>A key-value pair containing the response message given by the server for the request as the key and a list containing the names of the files as the value</returns>
        public IAsyncOperation<KeyValuePair<HttpResponseMessage, IList<string>>> ListFilesAsync(string folderPath)
        {
            return Task.Run(async () =>
            {
                string listUri = String.Format(ListUrlFormat, folderPath);

                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(listUri)))
                {
                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(requestMessage))
                    {
                        IList<string> files = new List<string>();
                        using (var inputStream = await response.Content.ReadAsInputStreamAsync())
                        {
                            using (var memStream = new MemoryStream())
                            {
                                using (Stream testStream = inputStream.AsStreamForRead())
                                {
                                    await testStream.CopyToAsync(memStream);
                                    memStream.Position = 0;
                                    using (StreamReader reader = new StreamReader(memStream))
                                    {
                                        //Get file name
                                        string result = reader.ReadToEnd();
                                        string[] parts = result.Split('"');

                                        //For each instance of the "name" field, the following string is the file name
                                        for (int i = 0; i < parts.Length; i++)
                                        {
                                            if (parts[i].Equals("name"))
                                            {
                                                files.Add(parts[i + 2]);
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
            accessToken = string.Empty;
            refreshToken = string.Empty;
            if (null != refreshTimer)
                refreshTimer.Dispose();
            if (null != httpClient)
            {
                httpClient.DefaultRequestHeaders.Clear();
                //httpClient.Dispose();
            }
            isLoggedIn = false;

            EventHandler<string> handler = TokensChangedEvent;

            if (null != handler)
            {
                handler(this, "Tokens Changed");
            }

            string logoutUri = string.Format(LogoutUriFormat, clientId, redirectUrl);
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

        private async void ReauthorizeOnTimer(object stateInfo)
        {
            await Reauthorize();
        }

        private void StartTimer()
        {
            //Set up timer to reauthenticate OneDrive login
            TimerCallback callBack = this.ReauthorizeOnTimer;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            TimeSpan dueTime = new TimeSpan(0);

            //use 50 minutes, as the accessToken expires after one hour by default
            TimeSpan period = new TimeSpan(0, 50, 0);
            refreshTimer = new Timer(callBack, autoEvent, dueTime, period);
        }

        private async Task<HttpResponseMessage> GetTokens(string accessCodeOrRefreshToken, string requestType, string grantType)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(TokenUri)))
            {
                string requestContent;
                //if (requestType == "code")
                requestContent = string.Format(TokenContentFormatAccess, clientId, redirectUrl, requestType, accessCodeOrRefreshToken, grantType);
                //else
                //    requestContent = string.Format(TokenContentFormatRefresh, clientId, redirectUrl, clientSecret, requestType, accessCodeOrRefreshToken, grantType);
                requestMessage.Content = new HttpStringContent(requestContent);
                requestMessage.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                using (HttpResponseMessage responseMessage = await httpClient.SendRequestAsync(requestMessage))
                {
                    string responseContentString = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        accessToken = GetAccessToken(responseContentString);
                        refreshToken = GetRefreshToken(responseContentString);
                        httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", accessToken);

                        TokensChangedEvent?.Invoke(this, "Tokens Changed");

                        isLoggedIn = true;
                    }
                    return responseMessage;
                }
            }

        }

        private string GetAccessToken(string responseContent)
        {
            string identifier = "\"access_token\":\"";
            int startIndex = responseContent.IndexOf(identifier) + identifier.Length;
            int endIndex = responseContent.IndexOf("\"", startIndex);
            return responseContent.Substring(startIndex, endIndex - startIndex);
        }

        private string GetRefreshToken(string responseContent)
        {
            string identifier = "\"refresh_token\":\"";
            int startIndex = responseContent.IndexOf(identifier) + identifier.Length;
            int endIndex = responseContent.IndexOf("\"", startIndex);
            return responseContent.Substring(startIndex, endIndex - startIndex);
        }

    }
}
