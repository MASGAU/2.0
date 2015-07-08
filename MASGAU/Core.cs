using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GameSaveInfo.Data;

namespace MASGAU
{
    class Core
    {
        private List<LocationFinders.ALocationFinder> LocationFinders = new List<LocationFinders.ALocationFinder>();

        public static bool AllUsersModes = true;

        private static SecondLanguage.Translator _Translator = SecondLanguage.Translator.Default;
        public static SecondLanguage.Translator Translator {
            get {
                return SecondLanguage.Translator.Default;
            }
        }

        static Core() {
            Translator.RegisterTranslationsByCulture(@"{0}Strings\*.po");
        }

        public Core() {

        }


        public string Initialize() {
            

            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resources = assembly.GetManifestResourceNames();
            StringBuilder output = new StringBuilder();
            foreach(string resource in resources) {
                if (!resource.EndsWith(".xml")) {
                    continue;
                }
                using (Stream stream = assembly.GetManifestResourceStream(resource)) {
                    DataRoot data = DataRoot.FromXml(stream);
                    output.AppendLine(data.ToXml());
                }
            }
            return output.ToString();
        }

        
    }
}
