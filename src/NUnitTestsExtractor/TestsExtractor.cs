using System;
using System.IO;
using System.Xml;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using NUnit.Engine;
using NUnit.Engine.Runners;
using NUnit.Engine.Services;

namespace NUnitTestsExtractor
{
    public class TestsExtractor
    {
        public XmlDocument ExtractTests(string testAssemblyPath)
        {
            if (string.IsNullOrWhiteSpace(testAssemblyPath))
            {
                throw new ArgumentException($"Given {nameof(testAssemblyPath)} was null or empty.", nameof(testAssemblyPath));
            }
            if (!File.Exists(testAssemblyPath))
            {
                throw new ArgumentException($"Could not find file '{testAssemblyPath}'.", nameof(testAssemblyPath));
            }

            // var parentPath = Directory.GetParent(testAssemblyPath);
            var package = new TestPackage(testAssemblyPath);
#if NETSTANDARD || NETCOREAPP
            var services = new ServiceContext();
            // services.Add(new DriverService());
            services.Add(new MyDriverService());
            services.Add(new ExtensionService());
            services.Add(new InProcessTestRunnerFactory());
            services.Add(new SettingsService(true));
            services.Add(new RecentFilesService());
            services.Add(new TestFilterService());
            services.Add(new ExtensionService());
            services.Add(new ProjectService());
            services.ServiceManager.StartServices();
            var runner = new MasterTestRunner(services, package);
#else
            var testEngine = TestEngineActivator.CreateInstance();
            var runner = testEngine.GetRunner(package);
#endif

            var nunitXml = runner.Explore(TestFilter.Empty);
            var session = new DiaSession(testAssemblyPath);
            foreach (XmlNode testNode in nunitXml.SelectNodes("//test-case"))
            {
                var className = testNode.Attributes["classname"]?.Value;
                var methodName = testNode.Attributes["methodname"]?.Value;
                var navigationData = session.GetNavigationData(className, methodName);
                var fileNameAttribute = testNode.OwnerDocument.CreateAttribute("filename");
                fileNameAttribute.Value = navigationData.FileName;
                testNode.Attributes.Append(fileNameAttribute);
                var lineNumberAttribute = testNode.OwnerDocument.CreateAttribute("linenumber");
                lineNumberAttribute.Value = navigationData.MinLineNumber.ToString();
                testNode.Attributes.Append(lineNumberAttribute);
            }
            return nunitXml.OwnerDocument;
        }
    }
}
