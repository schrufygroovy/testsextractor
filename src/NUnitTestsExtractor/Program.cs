using System;

namespace NUnitTestsExtractor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Expecting exactly 1 argument: the path to the test assembly.");
            }
            new TestsExtractor().ExtractTests(args[0]).Save("testcases.nunit.xml");
        }
    }
}
