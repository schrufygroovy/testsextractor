using System;

namespace TestsExtractor
{
    [Serializable]
    public class JsonTestCase
    {
        public string CodeFilePath { get; set; }

        public int LineNumber { get; set; }

        public string FullyQualifiedName { get; set; }

        public string DisplayName { get; set; }
    }
}
