using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Interfaces;
using IniParser;
using IniParser.Model;
using System.Reflection;
using System.Windows;
using Transitionals;
using Transitionals.Transitions;

namespace CloudDriveShell.Infrastructure.Services
{
    [Export(typeof(ICloudDriveConfigManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CloudDriveConfigManager : ICloudDriveConfigManager
    {
        private const string ConfigFileName = "CloudDriveShell.INI";

        public static string DataServerSectionConstant = "DataServer";


        public static string DataServerAddressConstant = "ADDRESS";

        public static string DataServerPortConstant = "PORT";

        public static string DataServerPathConstant = "PATH";


        public static string UpdateServerSectionConstant = "UpdateServer";


        public static string UpdateServerAddressConstant = "ADDRESS";

        public static string UpdateServerPortConstant = "PORT";


        public static string LoginConfigSectionConstant = "LoginInfo";


        public static string AutoLoginConstant = "AUTO";

        public static string RemeberInfoConstant = "REMEBER";

        public static string UserNameConstant = "USERNAME";

        public static string PasswordConstant = "PASSWORD";

        public static Transition TransitionToUse
        {
            get
            {
                return new FadeTransition { Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            }
        }

        public void WriteConfig(string section, string name, string value)
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            if (!File.Exists(fullPath))
                File.Create(fullPath);

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(fullPath);
            if (!data.Sections.ContainsSection(section))
            {
                data.Sections.AddSection(section);
            }
            if (data.Sections[section].ContainsKey(name))
            {
                data.Sections[section][name] = value;
            }
            else
            {
                data.Sections[section].AddKey(name, value);
            }
            parser.WriteFile(ConfigFileName, data, Encoding.UTF8);
        }

        public string ReadConfig(string section, string name)
        {
            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
                if (!File.Exists(fullPath))
                    File.Create(fullPath);

                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(fullPath, Encoding.UTF8);
                if (!data.Sections.ContainsSection(section)) return string.Empty;
                if (!data[section].ContainsKey(name)) return string.Empty;
                return data[section][name].Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

    }
}
