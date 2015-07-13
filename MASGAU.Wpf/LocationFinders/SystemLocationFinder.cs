using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SecondLanguage;

using GameSaveInfo.Data.Enums;
using MASGAU.Registry;

namespace MASGAU.LocationFinders {
    partial class SystemLocationFinder: ALocationFinder {
        #region Interop-related
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID {
            public UInt32 LowPart;
            public Int32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
            public int Size() {
                return Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES {
            public LUID luid;
            public UInt32 attributes;
        }
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        public static extern int OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess,
        ref IntPtr tokenhandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern int GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        public static extern int LookupPrivilegeValue(string lpsystemname, string lpname,
        [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
        public static extern int AdjustTokenPrivileges(IntPtr tokenhandle, int disableprivs,
        [MarshalAs(UnmanagedType.Struct)]ref TOKEN_PRIVILEGES Newstate, int bufferlength,
        ref TOKEN_PRIVILEGES PreviousState, ref int Returnlength);

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [Flags]
        public enum RegSAM {
            AllAccess = 0x000f003f
        }
        private const int REG_PROCESS_APPKEY = 0x00000001;
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegLoadAppKey(String hiveFile, out int hKey, RegSAM samDesired, int options, int reserved);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        public const uint TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const uint TOKEN_QUERY = 0x00000008;
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        public const uint HKEY_USERS = 0x80000003;
        public string shortname;
        #endregion

        private enum WindowsVersion {
            WindowsXP, WindowsVista
        }

        private bool UACEnabled = false;
        private WindowsVersion Version;

        public override void Initialize() {
            
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ProgramW6432"))) {
                this.AddPath(EnvironmentVariable.ProgramFiles, Environment.GetEnvironmentVariable("ProgramW6432"));
                this.AddPath(EnvironmentVariable.ProgramFilesX86, Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"));
            } else {
                this.AddPath(EnvironmentVariable.ProgramFiles, Environment.GetEnvironmentVariable("PROGRAMFILES"));
                //this.AddPath(EnvironmentVariable.ProgramFilesX86,Environment.GetEnvironmentVariable("PROGRAMFILES"));
            }
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAPPDATA"))) {
                Version = WindowsVersion.WindowsVista;
            } else {
                Version = WindowsVersion.WindowsXP;
            }

            this.AddPath(EnvironmentVariable.StartMenu, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

            // Not really used
            //common_program_files = Environment.GetEnvironmentVariable("COMMONPROGRAMFILES");

            foreach (DriveInfo look_here in DriveInfo.GetDrives()) {
                if (look_here.IsReady && (look_here.DriveType == DriveType.Fixed || look_here.DriveType == DriveType.Removable)) {
                    Drives.Add(look_here.Name);
                }
            }


            //host_name = Environment.GetEnvironmentVariable("COMPUTERNAME");
            this.AddPath(EnvironmentVariable.AllUsersProfile, Environment.GetEnvironmentVariable("ALLUSERSPROFILE"));

            if (Version == WindowsVersion.WindowsVista)
                this.AddPath(EnvironmentVariable.Public, Environment.GetEnvironmentVariable("PUBLIC"));

            this.AddPath(EnvironmentVariable.CommonApplicationData, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            if (Version != WindowsVersion.WindowsXP) {
                RegistryHandler uac_status = new RegistryHandler("local_machine", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false);
                if (uac_status.getValue("EnableLUA") == "1") {
                    UACEnabled = true;
                }
            }
            string[] split = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Split(Path.DirectorySeparatorChar);

            StringBuilder user_root = new StringBuilder(split[0]);
            if (Version == WindowsVersion.WindowsXP) {
                for (int i = 1; i < split.Length - 2; i++) {
                    user_root.Append(Path.DirectorySeparatorChar + split[i]);
                }
            } else {
                for (int i = 1; i < split.Length - 3; i++) {
                    user_root.Append(Path.DirectorySeparatorChar + split[i]);
                }
            }





            //Per-user variables
            loadUsersData("current_user", null);

            if (Core.AllUsersModes) {
                // All this crap lets me get data from other user's registries
                IntPtr token = new IntPtr(0);
                int retval = 0;

                TOKEN_PRIVILEGES TP = new TOKEN_PRIVILEGES();
                TOKEN_PRIVILEGES TP2 = new TOKEN_PRIVILEGES();
                LUID RestoreLuid = new LUID();
                LUID BackupLuid = new LUID();

                int return_length = 0;
                TOKEN_PRIVILEGES oldPriveleges = new TOKEN_PRIVILEGES();

                System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
                //IntPtr hndle = GetModuleHandle(null);

                //retval = OpenProcessToken(hndle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref token);
                retval = OpenProcessToken(process.Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref token);

                retval = LookupPrivilegeValue(null, SE_RESTORE_NAME, ref RestoreLuid);
                TP.PrivilegeCount = 1;
                TP.Privileges.attributes = SE_PRIVILEGE_ENABLED;
                TP.Privileges.luid = RestoreLuid;
                retval = AdjustTokenPrivileges(token, 0, ref TP, TP.Size(), ref oldPriveleges, ref return_length);
                
                if (retval == 0)
                    throw new Exception(Core.Translator["ProcessRestorePermissionError", retval.ToString()]);

                retval = LookupPrivilegeValue(null, SE_BACKUP_NAME, ref BackupLuid);
                TP2.PrivilegeCount = 1;
                TP2.Privileges.attributes = SE_PRIVILEGE_ENABLED;
                TP2.Privileges.luid = BackupLuid;
                retval = AdjustTokenPrivileges(token, 0, ref TP2, TP2.Size(), ref oldPriveleges, ref return_length);
                if (retval == 0)
                    throw new Exception(Core.Translator["ProcessBackupPermissionError", retval.ToString()]);


                Console.WriteLine(retval);

                foreach (DirectoryInfo user_folder in new DirectoryInfo(user_root.ToString()).GetDirectories()) {
                    if (user_folder.Name.ToLower() == "default user")
                        continue;
                    if (user_folder.Name.ToLower() == "default")
                        continue;
                    if (user_folder.Name.ToLower() == "all users")
                        continue;

                    string hive_file = Path.Combine(user_folder.FullName, "NTUSER.DAT");
                    if (!File.Exists(hive_file))
                        continue;


                    int h = RegLoadKey(HKEY_USERS, user_folder.Name, hive_file);
                    //int h = RegLoadAppKey(hive_file,out hKey, RegSAM.AllAccess,REG_PROCESS_APPKEY,0);

                    if (h == 32)
                        continue;

                    if (h != 0)
                        throw new Exception(Core.Translator["UserRegistryLoadError", user_folder.Name, h.ToString()]);

                    //sub_key = new RegistryHandler(hKey);
                    //sub_key = new RegistryHandler(RegRoot.users,user_folder.Name,false);
                    loadUsersData("users", user_folder.Name);
                    string result = RegUnLoadKey(HKEY_USERS, user_folder.Name).ToString();

                }
            }

            
        }

        private void loadUsersData(string reg_root, string path) {
            RegistryHandler user_key;
            if (String.IsNullOrEmpty(path))
                user_key = new RegistryHandler(reg_root, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", false);
            else
                user_key = new RegistryHandler(reg_root, path + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", false);

            string user_name;
            string[] split;
            try {
                split = user_key.getValue("AppData").Split(Path.DirectorySeparatorChar);
                if (Version == WindowsVersion.WindowsXP)
                    user_name = split[split.Length - 2];
                else
                    user_name = split[split.Length - 3];

                this.AddPath(EnvironmentVariable.AppData, user_key.getValue("AppData"),user_name);

                this.AddPath(EnvironmentVariable.Desktop, user_key.getValue("Desktop"),user_name);
                this.AddPath(EnvironmentVariable.StartMenu, user_key.getValue("Start Menu"), user_name);

                this.AddPath(EnvironmentVariable.LocalAppData, user_key.getValue("Local AppData"), user_name);

                //add_me.local_settings = user_key.GetValue("Local Settings").ToString();
                //add_me.start_menu = user_key.getValue("Start Menu");

                StringBuilder user_dir = new StringBuilder(split[0]);
                if (Version== WindowsVersion.WindowsXP) {
                    for (int i = 1; i < split.Length - 1; i++) {
                        user_dir.Append(Path.DirectorySeparatorChar + split[i]);
                    }
                } else {
                    for (int i = 1; i < split.Length - 2; i++) {
                        user_dir.Append(Path.DirectorySeparatorChar + split[i]);
                    }
                }
                this.AddPath(EnvironmentVariable.UserProfile, user_dir.ToString(), user_name);
                this.AddPath(EnvironmentVariable.UserDocuments, user_key.getValue("Personal"), user_name);

                if (Version == WindowsVersion.WindowsVista)
                    this.AddPath(EnvironmentVariable.VirtualStore, Path.Combine(user_key.getValue("Local AppData"), "VirtualStore"), user_name);

                user_key.close();

                if (String.IsNullOrEmpty(path))
                    user_key = new RegistryHandler(reg_root, @"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", false);
                else
                    user_key = new RegistryHandler(reg_root, path + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", false);

                string saved_games = user_key.getValue("{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}");
                if (!String.IsNullOrEmpty(saved_games)) {
                    if (saved_games.StartsWith("%USERPROFILE%")) {
                        saved_games = Path.Combine(user_dir.ToString(), saved_games.Substring(14));
                    }
                    this.AddPath(EnvironmentVariable.SavedGames, saved_games, user_name);
                } else {
                    if (Directory.Exists(Path.Combine(user_dir.ToString(), "Saved Games"))) {
                        this.AddPath(EnvironmentVariable.SavedGames, Path.Combine(user_dir.ToString(), "Saved Games"), user_name);
                    }
                }
                user_key.close();
                this.Users.Add(user_name);
            } catch (Exception) {
                return;
            }
        }

        //public static LocationPath translateToVirtualStore(string path) {
        //    LocationPath virtualstore_info = new LocationPath(EnvironmentVariable.LocalAppData, Path.Combine("VirtualStore", path.Substring(2).Trim(Path.DirectorySeparatorChar)));

        //    return virtualstore_info;
        //}

        //protected override DetectedLocations getPaths(LocationPath get_me) {
        //    //if(get_me.rel_root!= EnvironmentVariable.Public)
        //    // return new List<DetectedLocationPathHolder>();
        //    DetectedLocations return_me = new DetectedLocations();
        //    DetectedLocationPathHolder add_me;
        //    DirectoryInfo test;
        //    switch (get_me.EV) {
        //        case EnvironmentVariable.InstallLocation:
        //            LocationPath temp = new LocationPath(get_me);
        //            string[] chopped = temp.Path.Split(Path.DirectorySeparatorChar);
        //            for (int i = 0; i < chopped.Length; i++) {
        //                temp.ReplacePath(chopped[i]);
        //                for (int j = i + 1; j < chopped.Length; j++) {
        //                    temp.AppendPath(chopped[j]);
        //                }
        //                temp.EV = EnvironmentVariable.ProgramFiles;
        //                return_me.AddRange(getPaths(temp));
        //            }
        //            return_me.AddRange(base.getPaths(get_me));
        //            break;
        //        case EnvironmentVariable.ProgramFiles:
        //        case EnvironmentVariable.ProgramFilesX86:
        //            // Always checks both the VirtualStore and the real Program Files,
        //            // to make sure nothing is missed, especially in the case of old games
        //            // that may or may not use the VirtualStore
        //            if (!get_me.override_virtual_store && platform_version == "WindowsVista") {
        //                LocationPath virtualstore_info = new LocationPath(get_me);
        //                virtualstore_info.EV = EnvironmentVariable.LocalAppData;

        //                if (x64) {
        //                    virtualstore_info.ReplacePath(Path.Combine("VirtualStore", global.getFolder(EnvironmentVariable.ProgramFilesX86).BaseFolder.Substring(3), virtualstore_info.Path));
        //                    return_me.AddRange(getPaths(virtualstore_info));
        //                    virtualstore_info = new LocationPath(get_me);
        //                    virtualstore_info.EV = EnvironmentVariable.LocalAppData;
        //                }
        //                virtualstore_info.ReplacePath(Path.Combine("VirtualStore", global.getFolder(EnvironmentVariable.ProgramFiles).BaseFolder.Substring(3), virtualstore_info.Path));
        //                return_me.AddRange(getPaths(virtualstore_info));
        //            }

        //            if (x64) {
        //                if (!String.IsNullOrEmpty(get_me.Path))
        //                    test = new DirectoryInfo(Path.Combine(global.getFolder(EnvironmentVariable.ProgramFilesX86).BaseFolder, get_me.Path));
        //                else
        //                    test = new DirectoryInfo(global.getFolder(EnvironmentVariable.ProgramFilesX86).BaseFolder);
        //                if (test.Exists) {
        //                    add_me = new DetectedLocationPathHolder(get_me, global.getFolder(EnvironmentVariable.ProgramFilesX86).BaseFolder, null);
        //                    return_me.Add(add_me);
        //                }
        //            }

        //            if (!String.IsNullOrEmpty(get_me.Path))
        //                test = new DirectoryInfo(Path.Combine(global.getFolder(EnvironmentVariable.ProgramFiles).BaseFolder, get_me.Path));
        //            else
        //                test = new DirectoryInfo(global.getFolder(EnvironmentVariable.ProgramFiles).BaseFolder);

        //            if (test.Exists) {
        //                add_me = new DetectedLocationPathHolder(get_me, global.getFolder(EnvironmentVariable.ProgramFiles).BaseFolder, null);
        //                return_me.Add(add_me);
        //            }
        //            break;
        //        default:
        //            return base.getPaths(get_me);
        //    }
        //    return return_me;
        //}


        //protected override DetectedLocations getPaths(LocationRegistry get_me) {
        //    DetectedLocations return_me = new DetectedLocations();

        //    RegistryHandler reg;

        //    // This handles if the root is a registry key
        //    if (!String.IsNullOrEmpty(get_me.Key)) {
        //        reg = new RegistryHandler(get_me.Root, get_me.Key, false);

        //        if (reg.key_found) {
        //            try {
        //                string path;
        //                if (String.IsNullOrEmpty(get_me.Value))
        //                    path = reg.getValue("");
        //                else
        //                    path = reg.getValue(get_me.Value);

        //                if (!String.IsNullOrEmpty(path)) {
        //                    if (path.Contains("/"))
        //                        path = path.Split('/')[0].Trim();

        //                    if (path.Contains("\""))
        //                        path = path.Trim('\"').Trim();

        //                    if (Path.HasExtension(path))
        //                        path = Path.GetDirectoryName(path);

        //                    path = get_me.modifyPath(path);
        //                    if (Directory.Exists(path)) {
        //                        return_me.AddRange(Core.locations.interpretPath(new DirectoryInfo(path).FullName));
        //                    }
        //                }
        //            } catch (Exception e) {
        //                throw new TranslateableException("RegistryKeyLoadError", e);
        //            }
        //        }
        //    }
        //    return return_me;
        //}

        //// This is something...
        //const int CSIDL_COMMON_STARTMENU = 0x0016;

        //[DllImport("shell32.dll")]
        //static extern bool SHGetSpecialFolderPath(
        //IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);


        //protected override DetectedLocations getPaths(LocationShortcut get_me) {
        //    FileInfo the_shortcut;
        //    //StringBuilder start_menu;
        //    DetectedLocations return_me = new DetectedLocations();
        //    String path;

        //    List<string> paths = this.getPaths(get_me.ev);
        //    the_shortcut = null;

        //    foreach (string check_me in paths) {
        //        the_shortcut = new FileInfo(Path.Combine(check_me, get_me.path));
        //        if (the_shortcut.Exists)
        //            break;
        //    }



        //    if (the_shortcut != null && the_shortcut.Exists) {
        //        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
        //        IWshRuntimeLibrary.IWshShortcut link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(the_shortcut.FullName);

        //        try {
        //            path = Path.GetDirectoryName(link.TargetPath);
        //            path = get_me.modifyPath(path);
        //            return_me.AddRange(Core.locations.interpretPath(path));
        //        } catch { }
        //    }
        //    return return_me;
        //}

    }
}
