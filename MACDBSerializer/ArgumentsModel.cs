using CommandLineParser.Arguments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MACDBSerializer
{
    public class ArgumentsModel
    {
        [FileArgument('i', "input", Description = "Input file", FileMustExist = true, Optional = false)]
        public FileInfo InputFile { get; set; }

        [FileArgument('o', "output", Description = "Output file", FileMustExist = false, Optional = false)]
        public FileInfo OutputFile { get; set; }
    }
}
