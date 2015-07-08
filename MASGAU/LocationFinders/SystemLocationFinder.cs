using System;
using System.Collections.Generic;
using System.Text;

namespace MASGAU.LocationFinders
{
    // Global implementation of SystemLocationFinder
    partial class SystemLocationFinder: ALocationFinder
    {
        public List<String> Users = new List<string>();
        private List<String> Drives = new List<string>();
    }
}
