using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Splunk;

namespace SerilogDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            const string outputTemplate =
@"=== Meta ===
Timestamp=""{Timestamp}""
Level=""{Level}""

=== Context ===
Applikasjon=""{Applikasjon}""
ThreadId=""{ThreadId}""
Cool=""{Cool}""

=== Message ===
{Message}
";

            EnableSelfLog();

            var customFields = new CustomFields
            {
                CustomFieldList = new List<CustomField>
                {
                    new CustomField("MyCustomField", "MyCustomFieldValue")
                }
            };

            var loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.WithProperty("Applikasjon", "Betaling.Api")
                .Destructure.AsScalar<string>()
                .WriteTo.ColoredConsole(outputTemplate: outputTemplate)
                .WriteTo.EventCollector("http://localhost:8088/", "b9543dee-981a-4175-a3e9-963bbcebf677", outputTemplate: outputTemplate)
                //.WriteTo.EventLog(
                //    machineName: Environment.MachineName,
                //    logName: "Dbank",
                //    source: "Spv.Logging.Demo"
                //    )
                ;

            var logger = loggerConfiguration.CreateLogger();

            var dict = new Dictionary<string, string> { { "foo", "gdfg" }, { "bar", "asda" } };

            logger.Information("Dette er innholdet: {Dict}", dict);

            //var logger2 = logger.ForContext("Cool", "Yolo");

            //logger2.Information("Denne skal inneholde ForContext verdi");
            // a8dd6aa7-f44b-49a7-8224-209cf73dfc93
            Console.ReadLine();
        }

        public static void EnableSelfLog()
        {
            var selflogFilePath = "C:\\Log\\SerilogDemo\\selflog.txt";

            Directory.CreateDirectory(Path.GetDirectoryName(selflogFilePath));

            var streamWriter = new StreamWriter(
                new FileStream(
                    selflogFilePath,
                    FileMode.Append,
                    FileSystemRights.AppendData,
                    FileShare.ReadWrite,
                    16384,
                    FileOptions.None))
            {
                AutoFlush = true
            };

            SelfLog.Enable(TextWriter.Synchronized(streamWriter));
        }
    }

    internal class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Thread.CurrentThread.ManagedThreadId));
        }
    }
}
