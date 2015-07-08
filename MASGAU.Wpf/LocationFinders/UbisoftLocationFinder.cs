using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using GameSaveInfo.Data.Enums;
using MASGAU.Registry;

namespace MASGAU.LocationFinders {
    partial class UbisoftLocationFinder {

        public void Initialize() {
            // Global ubisoft save location
            DirectoryInfo ubisoft_save = null;
            RegistryHandler ubi_reg = new RegistryHandler("local_machine", @"SOFTWARE\Ubisoft\Launcher", false);
            if (!ubi_reg.key_found) {
                ubi_reg = new RegistryHandler("local_machine", @"SOFTWARE\Wow6432Node\Ubisoft\Launcher", false);
            }

            if (!String.IsNullOrEmpty(ubi_reg.getValue("InstallDir")) && Directory.Exists(Path.Combine(ubi_reg.getValue("InstallDir"), "savegames"))) {
                ubisoft_save = new DirectoryInfo(Path.Combine(ubi_reg.getValue("InstallDir"), "savegames"));
            } else if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ProgramW6432")) && Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Ubisoft", "Ubisoft Game Launcher"))) {
                ubisoft_save = new DirectoryInfo(Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Ubisoft", "Ubisoft Game Launcher", "savegames"));
            } else if (Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "Ubisoft", "Ubisoft Game Launcher"))) {
                ubisoft_save = new DirectoryInfo(Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "Ubisoft", "Ubisoft Game Launcher", "savegames"));
            }


            if (ubisoft_save != null && ubisoft_save.Exists) {
                this.AddPath(EnvironmentVariable.UbisoftSaveStorage, ubisoft_save, true);
            }

            // Per-user?

            //DirectoryInfo ubisoft_save = new DirectoryInfo(Path.Combine(add_me.getFolder(EnvironmentVariable.LocalAppData).BaseFolder, @"Ubisoft Game Launcher\savegame_storage"));
            //if (ubisoft_save.Exists) {
            //    add_me.setEvFolder(EnvironmentVariable.UbisoftSaveStorage, ubisoft_save);
            //}




        }
    }
}
