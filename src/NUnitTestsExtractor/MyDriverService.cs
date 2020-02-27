#if NETSTANDARD || NETCOREAPP
using System;
using NUnit.Engine;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Services;

namespace NUnitTestsExtractor
{
    public class MyDriverService : Service, IDriverService
    {
        public IFrameworkDriver GetDriver(AppDomain domain, string assemblyPath, string targetFramework, bool skipNonTestAssemblies)
        {
            return new MyNUnitDriver();
        }
    }
}
#endif
