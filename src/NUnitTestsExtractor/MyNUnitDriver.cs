﻿#if NETSTANDARD || NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Engine;
using NUnit.Engine.Drivers;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Internal;

namespace NUnitTestsExtractor
{
#pragma warning disable SA1310 // Field names should not contain underscore
#pragma warning disable SA1400 // Access modifier should be declared
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1612 // Element parameter documentation should match element parameters
#pragma warning disable SA1503 // Braces should not be omitted
#pragma warning disable SA1101 // Prefix local calls with this
#pragma warning disable SA1122 // Use string.Empty for empty strings
#pragma warning disable SA1629 // Documentation text should end with a period
    public class MyNUnitDriver : IFrameworkDriver
    {
        const string LOAD_MESSAGE = "Method called without calling Load first";
        const string INVALID_FRAMEWORK_MESSAGE = "Running tests against this version of the framework using this driver is not supported. Please update NUnit.Framework to the latest version.";
        const string FAILED_TO_LOAD_TEST_ASSEMBLY = "Failed to load the test assembly {0}";
        const string FAILED_TO_LOAD_NUNIT = "Failed to load the NUnit Framework in the test assembly";

        static readonly string CONTROLLER_TYPE = "NUnit.Framework.Api.FrameworkController";
        static readonly string LOAD_METHOD = "LoadTests";
        static readonly string EXPLORE_METHOD = "ExploreTests";
        static readonly string COUNT_METHOD = "CountTests";
        static readonly string RUN_METHOD = "RunTests";
        static readonly string RUN_ASYNC_METHOD = "RunTests";
        static readonly string STOP_RUN_METHOD = "StopRun";

        static ILogger log = InternalTrace.GetLogger(nameof(NUnitNetStandardDriver));

        Assembly _testAssembly;
        Assembly _frameworkAssembly;
        object _frameworkController;
        Type _frameworkControllerType;

        /// <summary>
        /// An id prefix that will be passed to the test framework and used as part of the
        /// test ids created.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Loads the tests in an assembly.
        /// </summary>
        /// <param name="frameworkAssembly">The NUnit Framework that the tests reference</param>
        /// <param name="testAssembly">The test assembly</param>
        /// <param name="settings">The test settings</param>
        /// <returns>An Xml string representing the loaded test</returns>
        public string Load(string testAssembly, IDictionary<string, object> settings)
        {
            var idPrefix = string.IsNullOrEmpty(ID) ? "" : ID + "-";

            var assemblyRef = AssemblyDefinition.ReadAssembly(testAssembly);

            _testAssembly = Assembly.Load(new AssemblyName(assemblyRef.FullName));
            if (_testAssembly == null)
                throw new NUnitEngineException(string.Format(FAILED_TO_LOAD_TEST_ASSEMBLY, assemblyRef.FullName));

            var nunitRef = assemblyRef.MainModule.AssemblyReferences.Where(reference => reference.Name.Equals("nunit.framework", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (nunitRef == null)
                throw new NUnitEngineException(FAILED_TO_LOAD_NUNIT);

            var nunit = Assembly.Load(new AssemblyName(nunitRef.FullName));
            if (nunit == null)
                throw new NUnitEngineException(FAILED_TO_LOAD_NUNIT);

            _frameworkAssembly = nunit;

            _frameworkController = CreateObject(CONTROLLER_TYPE, _testAssembly, idPrefix, settings);
            if (_frameworkController == null)
                throw new NUnitEngineException(INVALID_FRAMEWORK_MESSAGE);

            _frameworkControllerType = _frameworkController.GetType();

            log.Info("Loading {0} - see separate log file", _testAssembly.FullName);
            return ExecuteMethod(LOAD_METHOD) as string;
        }

        /// <summary>
        /// Counts the number of test cases for the loaded test assembly
        /// </summary>
        /// <param name="filter">The XML test filter</param>
        /// <returns>The number of test cases</returns>
        public int CountTestCases(string filter)
        {
            CheckLoadWasCalled();
            object count = ExecuteMethod(COUNT_METHOD, filter);
            return count != null ? (int)count : 0;
        }

        /// <summary>
        /// Executes the tests in an assembly.
        /// </summary>
        /// <param name="listener">An ITestEventHandler that receives progress notices</param>
        /// <param name="filter">A filter that controls which tests are executed</param>
        /// <returns>An Xml string representing the result</returns>
        public string Run(ITestEventListener listener, string filter)
        {
            CheckLoadWasCalled();
            log.Info("Running {0} - see separate log file", _testAssembly.FullName);
            Action<string> callback = listener != null ? listener.OnTestEvent : (Action<string>)null;
            return ExecuteMethod(RUN_METHOD, new[] { typeof(Action<string>), typeof(string) }, callback, filter) as string;
        }

        /// <summary>
        /// Executes the tests in an assembly asyncronously.
        /// </summary>
        /// <param name="callback">A callback that receives XML progress notices</param>
        /// <param name="filter">A filter that controls which tests are executed</param>
        public void RunAsync(Action<string> callback, string filter)
        {
            CheckLoadWasCalled();
            log.Info("Running {0} - see separate log file", _testAssembly.FullName);
            ExecuteMethod(RUN_ASYNC_METHOD, new[] { typeof(Action<string>), typeof(string) }, callback, filter);
        }

        /// <summary>
        /// Cancel the ongoing test run. If no  test is running, the call is ignored.
        /// </summary>
        /// <param name="force">If true, cancel any ongoing test threads, otherwise wait for them to complete.</param>
        public void StopRun(bool force)
        {
            ExecuteMethod(STOP_RUN_METHOD, force);
        }

        /// <summary>
        /// Returns information about the tests in an assembly.
        /// </summary>
        /// <param name="filter">A filter indicating which tests to include</param>
        /// <returns>An Xml string representing the tests</returns>
        public string Explore(string filter)
        {
            CheckLoadWasCalled();

            log.Info("Exploring {0} - see separate log file", _testAssembly.FullName);
            return ExecuteMethod(EXPLORE_METHOD, filter) as string;
        }

        void CheckLoadWasCalled()
        {
            if (_frameworkController == null)
                throw new InvalidOperationException(LOAD_MESSAGE);
        }

        object CreateObject(string typeName, params object[] args)
        {
            var typeinfo = _frameworkAssembly.DefinedTypes.FirstOrDefault(t => t.FullName == typeName);
            if (typeinfo == null)
            {
                log.Error("Could not find type {0}", typeName);
            }
            return Activator.CreateInstance(typeinfo.AsType(), args);
        }

        object ExecuteMethod(string methodName, params object[] args)
        {
            var method = _frameworkControllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return ExecuteMethod(method, args);
        }

        object ExecuteMethod(string methodName, Type[] ptypes, params object[] args)
        {
            var method = _frameworkControllerType.GetMethod(methodName, ptypes);
            return ExecuteMethod(method, args);
        }

        object ExecuteMethod(MethodInfo method, params object[] args)
        {
            if (method == null)
            {
                throw new NUnitEngineException(INVALID_FRAMEWORK_MESSAGE);
            }
            return method.Invoke(_frameworkController, args);
        }
    }
#pragma warning restore SA1400 // Access modifier should be declared
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1612 // Element parameter documentation should match element parameters
#pragma warning restore SA1503 // Braces should not be omitted
#pragma warning restore SA1101 // Prefix local calls with this
#pragma warning restore SA1122 // Use string.Empty for empty strings
#pragma warning restore SA1629 // Documentation text should end with a period
}
#endif
