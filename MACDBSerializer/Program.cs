using CommandLineParser.Exceptions;
using CsvHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MessagePack;
using System.IO.Compression;

namespace MACDBSerializer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<MACRecord> _records;
            List<MACRecordBin> _recordsSerializable;

            var parser = new CommandLineParser.CommandLineParser();
            var arg = new ArgumentsModel();
            parser.ExtractArgumentAttributes(arg);

            try
            {
                parser.ParseCommandLine(args);
                parser.ShowParsedArguments();
            }
            catch (CommandLineException ex)
            {
                Console.WriteLine(ex.Message);
                parser.ShowUsage();
            }

            using (var file = new FileStream(arg.InputFile.FullName, FileMode.Open))
            {
                using (var text = new StreamReader(file))
                {
                    using (var csv = new CsvReader(text, CultureInfo.InvariantCulture))
                    {
                        csv.Parser.Configuration.Delimiter = ",";
                        _records = new List<MACRecord>(csv.GetRecords<MACRecord>());
                    }
                }
            }

            _recordsSerializable = new List<MACRecordBin>(_records.Count);

            foreach (var record in _records)
            {
                _recordsSerializable.Add(new MACRecordBin() { 
                    Assigment = Convert.ToUInt32(record.Assigment, 16),
                    OrganizationName = record.OrganizationName
                });
            }

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(_recordsSerializable);

            using (var writer = new StreamWriter(arg.OutputFile.FullName, false))
            {
                writer.BaseStream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
