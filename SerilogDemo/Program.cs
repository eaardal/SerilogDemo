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
            EnableSelfLog();

            const string outputTemplate =
@"=== Meta ===
Timestamp=""{Timestamp}""
Level=""{Level}""

=== Context ===
Application=""{Applikasjon}""
ThreadId=""{ThreadId}""

=== Message ===
{Message}
";
            
            var loggerConfiguration = new LoggerConfiguration();

            loggerConfiguration
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.WithProperty("Application", "MyApp")
                .WriteTo.ColoredConsole(outputTemplate: outputTemplate)
                .WriteTo.EventCollector("http://localhost:8088/", "b9543dee-981a-4175-a3e9-963bbcebf677", outputTemplate: outputTemplate)
                ;

            var logger = loggerConfiguration.CreateLogger();

            var dict = new Dictionary<string, string> { { "foo", "gdfg" }, { "bar", "asda" } };

            logger.Information("Some content: {Dict}", dict);
            
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
