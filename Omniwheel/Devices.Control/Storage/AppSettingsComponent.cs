using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Windows.Storage;

namespace Devices.Control.Storage
{
    public class AppSettingsComponent : Controllable
    {
        ApplicationDataContainer settings;

        public AppSettingsComponent(): base("AppSettings")
        {
            this.settings = ApplicationData.Current.LocalSettings;
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "SETTINGS HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "SETTINGS LIST : Returns a list of all available Settings, but no values.");
            data.AddMultiPartValue("Help", "SETTINGS GET <SettingName> : Returns a setting  given by name.");
            data.AddMultiPartValue("Help", "SETTINGS SET <SettingName>:<SettingValue> : Adds or sets a setting  and value.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (data.ResolveParameter(nameof(MessageContainer.FixedPropertyNames.Action), 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data).ConfigureAwait(false);
                    break;
                case "GET":
                    await SettingsComponentGetSetting(data).ConfigureAwait(false);
                    break;
                case "SET":
                    await SettingsComponentSetSetting(data).ConfigureAwait(false);
                    break;
                case "LIST":
                    await SettingsComponentGetAllSettingNames(data).ConfigureAwait(false);
                    break;
            }
        }

        #region command handling
        private async Task SettingsComponentGetAllSettingNames(MessageContainer data)
        {
            data.AddValue("Settings", await GetSettingNames().ConfigureAwait(false));
            await HandleOutput(data);
        }

        private async Task SettingsComponentGetSetting(MessageContainer data)
        {
            object settingValue;
            string settingName = data.ResolveParameter("Name", 0);
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                settings.Values.TryGetValue(settingName, out settingValue);
                data.AddValue(settingName, settingValue);
                await HandleOutput(data);
            }
        }

        private async Task SettingsComponentSetSetting(MessageContainer data)
        {
            string settingName = data.ResolveParameter("Name", 0);
            string settingValue = data.ResolveParameter("Value", 1);
            
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                settings.Values[settingName] = settingValue;
            }
            await Task.CompletedTask;
        }
        #endregion

        #region public handling
        public async Task<ICollection<string>> GetSettingNames()
        {
            await Task.CompletedTask;
            return settings.Values.Keys;
        }

        public ApplicationDataContainerSettings ApplicationSettings
        {
            get { return this.settings.Values as ApplicationDataContainerSettings; }
        }
        #endregion

    }
}
