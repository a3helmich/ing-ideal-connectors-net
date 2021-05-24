using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Test.Prepare.Utils
{
    public class TestPrepare
    {
        public static void CopyApplicationSettingsToActiveConfiguration()
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetCallingAssembly().Location);

            var activeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var appSettings = config.AppSettings;

            foreach (var appKey in appSettings.Settings.AllKeys)
            {
                if (activeConfig.AppSettings.Settings.AllKeys.Any(activeAppKey => activeAppKey == appKey))
                {
                    activeConfig.AppSettings.Settings[appKey].Value = appSettings.Settings[appKey].Value;

                    continue;
                }

                activeConfig.AppSettings.Settings.Add(appKey, appSettings.Settings[appKey].Value);
            }

            activeConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
