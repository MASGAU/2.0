using System;
using System.Collections.Generic;
using System.Text;
using GameSaveInfo.Data.Locations;
using GameSaveInfo.Data.Enums;

namespace MASGAU.LocationFinders
{
    abstract class ALocationFinder
    {
        private const string GLOBAL_CONST = "_#%GLOBAL%$_";

        private Dictionary<string,Dictionary<EnvironmentVariable, string>> Paths =
            new Dictionary<string, Dictionary<GameSaveInfo.Data.Enums.EnvironmentVariable, string>>();


        public abstract void Initialize();

        protected void AddPath(EnvironmentVariable ev, string path, string user = null) {
            if (user == null)
                user = GLOBAL_CONST;

            if(!Paths.ContainsKey(user))
                this.Paths.Add(user, new Dictionary<EnvironmentVariable,string>());

            Dictionary<EnvironmentVariable, string> entry = this.Paths[user];

            entry[ev] = path;

        }
    }
}
