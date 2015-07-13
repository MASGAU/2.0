using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using GameSaveInfo.Data;

namespace MASGAU {
    class Core : Progress<int> {
        private List<LocationFinders.ALocationFinder> LocationFinders = new List<LocationFinders.ALocationFinder>();

        private IProgress<ProcessStatus> Progress;

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

        public Core(IProgress<ProcessStatus> progress) {
            this.Progress = progress;
        }


        async public Task<string> Initialize() {
            StringBuilder output = new StringBuilder();
            await Task.Run(() => {
                List<DataRoot> roots = new List<DataRoot>();
                ProcessStatus status = new ProcessStatus();

                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resources = assembly.GetManifestResourceNames();

                foreach (string resource in resources) {
                    if (!resource.EndsWith(".xml")) {
                        continue;
                    }
                    status.Message = Translator.Translate("Loading game data...");
                    Progress.Report(status);
                    using (Stream stream = assembly.GetManifestResourceStream(resource)) {
                        DataRoot data = DataRoot.FromXml(stream);
                        output.AppendLine(data.ToXml());
                        roots.Add(data);
                    }
                }
                status.MaxValue = roots.Count;
                for (int i = 0; i < roots.Count; i++) {
                    status.Value = i;
                    status.Message = i.ToString();
                    Progress.Report(status);
                }
            });
            return output.ToString();
        }


    }
}
