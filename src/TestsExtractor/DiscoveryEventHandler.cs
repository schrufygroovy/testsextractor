using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestsExtractor
{
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
            this.waitHandle.Set();
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
