using System;
using System.Reflection;
using NUnit.Common;
using NUnit.Engine.Drivers;
using NUnit.Engine.Extensibility;

namespace NUnitTestsExtractor
{
    public class MyDriverFactory : IDriverFactory
    {
        private const string NUNITFRAMEWORK = "nunit.framework";

        public bool IsSupportedTestFramework(AssemblyName reference)
        {
            return NUNITFRAMEWORK.Equals(reference.Name, StringComparison.OrdinalIgnoreCase) && reference.Version.Major == 3;
        }

#if !NETSTANDARD1_6 && !NETSTANDARD2_0 && !NETCOREAPP
        public IFrameworkDriver GetDriver(AppDomain domain, AssemblyName reference)
        {
            Guard.ArgumentValid(this.IsSupportedTestFramework(reference), "Invalid framework", "reference");

            return new NUnit3FrameworkDriver(domain, reference);
        }
#else
        public IFrameworkDriver GetDriver(AssemblyName reference)
        {
            Guard.ArgumentValid(this.IsSupportedTestFramework(reference), "Invalid framework", "reference");

            return new MyNUnitDriver();
        }
#endif
    }
}
