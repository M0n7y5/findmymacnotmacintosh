using System;
using System.Collections.Generic;
using System.Text;

namespace FindMyMACNotMacintosh.Models
{
    public class ScanResultWithProgress
    {
        public int Progress { get; set; }

        public NetworkDevice Device { get; set; }

    }
}
