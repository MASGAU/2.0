using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GameSaveInfo.Data;

namespace MASGAU
{
    class MASGAU
    {
        public MASGAU() {

        }


        public string Init() {
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
