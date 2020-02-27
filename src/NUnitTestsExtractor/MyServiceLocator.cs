using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Engine;

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
            return this.serviceLocator.GetService<T>();
        }

        public object GetService(Type serviceType)
        {
            return this.serviceLocator.GetService(serviceType);
        }
    }
}
