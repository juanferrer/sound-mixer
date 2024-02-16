using System;
using System.Configuration;

namespace SoundMixer.Miscellanea
{
    public static class Settings
    {
        private static AppSettingsReader reader = new AppSettingsReader();

        public static string GetSetting(Enums.Setting key)
        {
            return ConfigurationManager.AppSettings[key.ToString()];
        }

        public static void SetSetting(Enums.Setting key, string value)
        {
            ConfigurationManager.AppSettings[key.ToString()] = value;
        }
    }
}
