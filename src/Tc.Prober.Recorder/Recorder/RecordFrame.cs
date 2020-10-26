using System;
using System.Linq;


namespace Tc.Prober.Recorder
{
    public class RecordFrame<P>
    {
        public long Stamp { get; set; }
        public P Object { get; set; }
    }
}
