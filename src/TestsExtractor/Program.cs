using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;

namespace TestsExtractor
{
    public class Program
    {
        public const string DefaultRunSettings = "<RunSettings><RunConfiguration></RunConfiguration></RunSettings>";

        public static void Main(string[] args)
        {
            var testAdapterDllPath = args[0];
            var testAssemblyDllPath = args[1];

            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"log.txt");
            var executedTestDllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var consoleExePath = Path.Combine(executedTestDllLocation, "testplatform", "vstest.console.dll");
            IVsTestConsoleWrapper consoleWrapper = new VsTestConsoleWrapper(consoleExePath, new ConsoleParameters { LogFilePath = logFilePath });

            consoleWrapper.StartSession();
            consoleWrapper.InitializeExtensions(new List<string>() { testAdapterDllPath });

            var testCases = DiscoverTests(new List<string>() { testAssemblyDllPath }, consoleWrapper);

            Console.WriteLine("Discovered Tests Count: " + testCases?.Count());
            var jsonTestCases = testCases.Select(ToJsonTestCase).ToList();

            var json = JsonConvert.SerializeObject(jsonTestCases, Formatting.Indented);
            File.WriteAllText("result.json", json);
        }

        public static IEnumerable<TestCase> DiscoverTests(IEnumerable<string> sources, IVsTestConsoleWrapper consoleWrapper)
        {
            var waitHandle = new AutoResetEvent(false);
            var handler = new DiscoveryEventHandler(waitHandle);
            consoleWrapper.DiscoverTests(sources, DefaultRunSettings, handler);

            waitHandle.WaitOne();

            return handler.DiscoveredTestCases;
        }

        public static JsonTestCase ToJsonTestCase(TestCase testCase)
        {
            return new JsonTestCase
            {
                CodeFilePath = testCase.CodeFilePath,
                DisplayName = testCase.DisplayName,
                FullyQualifiedName = testCase.FullyQualifiedName,
                LineNumber = testCase.LineNumber
            };
        }
    }
}
