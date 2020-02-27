using System;
using System.IO;
using System.Xml;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using NUnit.Engine;
using NUnit.Engine.Runners;

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
            var testEngine = TestEngineActivator.CreateInstance();
            // var runner = testEngine.GetRunner(package);
            var runner = new MasterTestRunner(new MyServiceLocator(testEngine.Services), package);
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
