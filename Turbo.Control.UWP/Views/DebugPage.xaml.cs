using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Devices.Controllers.Base;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Turbo.Control.UWP.Views
{

    public sealed partial class DebugPage : Page
    {
        DebugController debugController;

        public DebugPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            debugController = await DebugController.GetNamedInstance<DebugController>("DebugController", string.Empty);
            debugController.OnDataReceived += DebugController_OnReceivedTextUpdated;
            debugController.OnDataReceived += DebugController_OnSentTextUpdated;
            txtTextReceived.Text = debugController.DataReceived;
            txtTextSent.Text = debugController.DataSent;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            debugController.OnDataReceived -= DebugController_OnReceivedTextUpdated;
            debugController.OnDataSent -= DebugController_OnSentTextUpdated;
            base.OnNavigatedFrom(e);
        }

        private async void DebugController_OnSentTextUpdated(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtTextSent.Text = debugController.DataSent;
            });
        }

        private async void DebugController_OnReceivedTextUpdated(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtTextReceived.Text = debugController.DataReceived;
            });
        }

        private void btnTextReceivedClear_Click(object sender, RoutedEventArgs e)
        {
            debugController?.ClearReceivedBuffer();
        }

        private void btnTextSentClear_Click(object sender, RoutedEventArgs e)
        {
            debugController?.ClearSentBuffer();
        }

        private async void btnCommandAction_Click(object sender, RoutedEventArgs e)
        {
            JsonObject data;
            if (JsonObject.TryParse(txtTextCommand.Text, out data))
            {
                await ControllerHandler.Send(data);
            }
            else
            {
                ContentDialog invalidJsonDialog = new ContentDialog()
                {
                    Title = "Json Command not valid",
                    Content = "Please specify a valid Json string to be sent to the device host.",
                    PrimaryButtonText = "Ok"
                };
                await invalidJsonDialog.ShowAsync();
            }
        }
    }
}
