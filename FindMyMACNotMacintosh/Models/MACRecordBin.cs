using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;



namespace FindMyMACNotMacintosh.Models
{
    [MessagePackObject]
    public class MACRecordBin
    {
        // Registry,Assignment,Organization Name,Organization Address
        [Key(0)]
        public uint Assigment { get; set; }

        //[Name("Organization Name")]
        [Key(1)]
        public string OrganizationName { get; set; }
    }
}
