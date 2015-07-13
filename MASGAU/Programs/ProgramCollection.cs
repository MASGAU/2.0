using System;
using System.Collections.Generic;
using System.Text;
using GameSaveInfo.Data.Programs;
namespace MASGAU.Programs
{
    class ProgramCollection
    {
        private Dictionary<GameSaveInfo.Data.AProgramVersionID, ProgramFinder> InnerList =
            new Dictionary<GameSaveInfo.Data.AProgramVersionID, ProgramFinder>();




        public ProgramFinder Add(ProgramVersion program) {
            ProgramFinder finder = new ProgramFinder(program);
            InnerList[program] = finder;
            return finder;
        }

    }
}
