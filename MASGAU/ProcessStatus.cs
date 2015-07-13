using System;
using System.Collections.Generic;
using System.Text;

namespace MASGAU
{
    struct ProcessStatus
    {
        public string Message;
        public int MinValue;
        public int MaxValue;
        public Nullable<int> Value;

        public ProcessStatus(int min_value = 0, int max_value= 0) {
            Message = "";
            MinValue = min_value;
            MaxValue = max_value;
            Value = null;
        }
    }
}
