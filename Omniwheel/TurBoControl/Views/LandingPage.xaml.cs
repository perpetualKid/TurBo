using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Common.Communication;
using Common.Communication.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TurBoControl.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {
        SocketClient socketClient;

        public LandingPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (socketClient == null)
                socketClient = new SocketClient();
            if (socketClient.ConnectionStatus != ConnectionStatus.Connected)
            {
                ChannelBase channel = await socketClient.Connect("turbo", "8027", DataFormat.Text);
                channel.OnMessageReceived += SocketClient_OnMessageReceived;
            }
                    await socketClient.Send(Guid.Empty, "ECHO");
                //await SocketClient.Disconnect();
            //                await Task.Run(() => JsonStreamReader.ReadEndless(file.OpenStreamForReadAsync().Result));

        }

        private void SocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //            string text = (e as StringMessageArgs).Message;
            System.Diagnostics.Debug.WriteLine((e as StringMessageArgs).Parameters?.Length);
        }


    }
}
