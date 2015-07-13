using System;
using System.Collections.Generic;
using System.Text;

namespace MASGAU.Programs
{
    class ProgramFinder
    {
        public GameSaveInfo.Data.AProgramVersionID ID { get { return Program; } }


        private GameSaveInfo.Data.Programs.ProgramVersion Program;

        public ProgramFinder(GameSaveInfo.Data.Programs.ProgramVersion program) {
            this.Program = program;
        }
    }
}
