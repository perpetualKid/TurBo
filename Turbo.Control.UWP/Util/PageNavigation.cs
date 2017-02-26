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
        public static async Task<bool> ConnectionNeeded(this Page page, Type destinationPageType, object parameter)
        {
            if (!ControllerHandler.Connected)
            {
                ContentDialog missingClientId = new ContentDialog()
                {
                    Title = "OneDrive ClientId missing",
                    Content = "Please specify a valid OneDrive ClientId and ClientSecret.\r\n\r\nWhen you click OK you will be redirected to the Application Settings page.",
                    PrimaryButtonText = "Ok"
                };

                ContentDialogResult result = await missingClientId.ShowAsync();
                page.Frame.Navigate(typeof(AppSettingsPage), parameter, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                return false;
            }
            return true;
        }
    }
}
