using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MACDBSerializer
{

    public class MACRecord
    {
        // Registry,Assignment,Organization Name,Organization Address
        [Index(0)]
        public string Registry { get; set; }

        [Index(1)]
        public string Assigment { get; set; }

        //[Name("Organization Name")]
        [Index(2)]
        public string OrganizationName { get; set; }

        //[Name("Organization Address")]
        [Index(3)]
        public string OranizationAddress { get; set; }
    }
}
