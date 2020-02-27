using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Engine;
using NUnit.Engine.Extensibility;

namespace NUnitTestsExtractor
{
    public class MyServiceLocator : IServiceLocator
    {
        private readonly IServiceLocator serviceLocator;

        public MyServiceLocator(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public T GetService<T>()
            where T : class
        {
            if (typeof(T) == typeof(IDriverFactory))
            {
                return new MyDriverFactory() as T;
            }
            return this.serviceLocator.GetService<T>();
        }

        public object GetService(Type serviceType)
        {
            return this.serviceLocator.GetService(serviceType);
        }
    }
}
