using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealPolicePlugin.Core
{
    class Configuration
    {

        public const string KEY_SECTION = "KeyBindings";


        private InitializationFile _Ini = null;
        private static Configuration _Instance = null;
        private KeysConverter _KeysConverter = null;


        private Configuration()
        {
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/RealPolice.ini");
            ini.Create();
            this._Ini = ini;
            this._KeysConverter = new KeysConverter();
        }


        public static Configuration Instance
        {
            get
            {
                if (null == Configuration._Instance)
                {
                    Configuration._Instance = new Configuration();
                }

                return Configuration._Instance;
            }
        }

        public string Read(string sectionName, string itemName, string defaultValue)
        {
            return this._Ini.ReadString(sectionName, itemName, defaultValue);
        }

        public string Read(string sectionName, string itemName)
        {
            return this._Ini.ReadString(sectionName, itemName);
        }


        public Keys ReadKey(string itemName, string defaultKey)
        {
            return (Keys)this._KeysConverter.ConvertFromString(this.Read(Configuration.KEY_SECTION, itemName, defaultKey));
        }


    }
}
