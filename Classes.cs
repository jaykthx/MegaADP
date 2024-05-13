using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ADP_
{
    public class ADP
    {
        public required Header header = new();
        public required ObservableCollection<Function> functions { get; set; } = new();
    }

    public class Function //make a struct with all the values from an adp function
    {
        public float time_seconds { get; set; }
        public int time_frames { get; set; }
        public bool alt_pv_flag { get; set; }
        public int ID { get; set; }
        public bool is_30_fps { get; set; } // Necessary...
        public float Value { get; set; }
        public string? name { get; set; }
    }
    public class Header
    {
        public long function_count { get; set; }
        public long data_length { get; set; }
        public long offset { get; set; }
    }
}
