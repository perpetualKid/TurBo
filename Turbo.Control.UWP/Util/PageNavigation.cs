using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devices.Controllers.Base;
using Turbo.Control.UWP.Views;
using Windows.UI.Xaml.Controls;

namespace Turbo.Control.UWP.Util
{
    public static class PageNavigation
    {
        public static async Task<bool> CheckConnection(this Page page, object parameter)
        {
            if (!ControllerHandler.Connected)
            {
                ContentDialog missingClientId = new ContentDialog()
                {
                    Title = "Not Connected!",
                    Content = "Please connect to device first.",
                    PrimaryButtonText = "Ok"
                };

                ContentDialogResult result = await missingClientId.ShowAsync();
            }
            return ControllerHandler.Connected;
        }
    }
}
