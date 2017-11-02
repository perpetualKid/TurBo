using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Devices.Components;
using Windows.Storage;

namespace Turbo.BrickPi.Components.Storage
{
    public class AppSettingsComponent : ComponentBase
    {
        ApplicationDataContainer settings;

        public AppSettingsComponent(): base("AppSettings")
        {
            this.settings = ApplicationData.Current.LocalSettings;
        }

        #region command handling
        [Action("List")]
        [ActionHelp(" Returns a list of all available Settings, but no values.")]
        private async Task ListSettingNames(MessageContainer data)
        {
            data.AddValue("Settings", await GetSettingNames().ConfigureAwait(false));
            await ComponentHandler.HandleOutput(data);
        }

        [Action("Get")]
        [ActionParameter("SettingName")]
        [ActionHelp("Returns a setting  given by name.")]
        private async Task GetSettingByName(MessageContainer data)
        {
            object settingValue;
            string settingName = data.ResolveParameter("Name", 0);
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                settings.Values.TryGetValue(settingName, out settingValue);
                data.AddValue(settingName, settingValue);
                await ComponentHandler.HandleOutput(data);
            }
        }

        [Action("Set")]
        [ActionParameter("SettingName")]
        [ActionParameter("SettingValue")]
        [ActionHelp("Adds or sets a setting  and value.")]
        private async Task SetSettingByName(MessageContainer data)
        {
            string settingName = data.ResolveParameter("Name", 0);
            string settingValue = data.ResolveParameter("Value", 1);
            
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                settings.Values[settingName] = settingValue;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }
        #endregion

        #region public handling
        public Task<ICollection<string>> GetSettingNames()
        {
            return Task.FromResult<ICollection<string>>(settings.Values.Keys);
        }

        public ApplicationDataContainerSettings ApplicationSettings
        {
            get { return this.settings.Values as ApplicationDataContainerSettings; }
        }
        #endregion

    }
}
