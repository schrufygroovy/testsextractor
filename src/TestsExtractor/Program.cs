using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace VstestTest
{
    public class Program
    {
        public const string DefaultRunSettings = "<RunSettings><RunConfiguration></RunConfiguration></RunSettings>";

        public static void Main(string[] args)
        {
            var consoleExePath = args[0];
            var testAdapterDllPath = args[1];
            var testAssemblyDllPath = args[2];

            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"log.txt");
            IVsTestConsoleWrapper consoleWrapper = new VsTestConsoleWrapper(consoleExePath, new ConsoleParameters { LogFilePath = logFilePath });

            consoleWrapper.StartSession();
            consoleWrapper.InitializeExtensions(new List<string>() { testAdapterDllPath });

            var testCases = DiscoverTests(new List<string>() { testAssemblyDllPath }, consoleWrapper);

            Console.WriteLine("Discovered Tests Count: " + testCases?.Count());
            foreach (var tc in testCases)
            {
                Console.WriteLine();
                Console.WriteLine($"DisplayName={tc.DisplayName}"); 

                Console.WriteLine($"CodeFilePath={tc.CodeFilePath}");
                Console.WriteLine($"LineNumber={tc.LineNumber}");

                Console.WriteLine($"ExecutorUri={tc.ExecutorUri}");
                Console.WriteLine($"FullyQualifiedName={tc.FullyQualifiedName}");
                Console.WriteLine($"Id={tc.Id}");
                Console.WriteLine($"Source={tc.Source}");
            }
            Console.WriteLine();
        }

        public static IEnumerable<TestCase> DiscoverTests(IEnumerable<string> sources, IVsTestConsoleWrapper consoleWrapper)
        {
            var waitHandle = new AutoResetEvent(false);
            var handler = new DiscoveryEventHandler(waitHandle);
            consoleWrapper.DiscoverTests(sources, DefaultRunSettings, handler);

            waitHandle.WaitOne();

            return handler.DiscoveredTestCases;
        }

        public class DiscoveryEventHandler : ITestDiscoveryEventsHandler
        {
            private AutoResetEvent waitHandle;

            public DiscoveryEventHandler(AutoResetEvent waitHandle)
            {
                this.waitHandle = waitHandle;
                this.DiscoveredTestCases = new List<TestCase>();
            }

            public List<TestCase> DiscoveredTestCases { get; private set; }

            public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
            {
                Console.WriteLine("Discovery: " + discoveredTestCases.FirstOrDefault()?.DisplayName);

                if (discoveredTestCases != null)
                {
                    this.DiscoveredTestCases.AddRange(discoveredTestCases);
                }
            }

            public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
            {
                if (lastChunk != null)
                {
                    this.DiscoveredTestCases.AddRange(lastChunk);
                }

                Console.WriteLine("DiscoveryComplete");
                waitHandle.Set();
            }

            public void HandleLogMessage(TestMessageLevel level, string message)
            {
                Console.WriteLine("Discovery Message: " + message);
            }

            public void HandleRawMessage(string rawMessage)
            {
                // No op
            }
        }
    }
}
