using SensitiveWords;
using System.Collections.Generic;

namespace SensitiveWordsTests
{
    internal class Test
    {
        public string P1 { get; set; }
        public int P2 { get; set; }
        public string P3 { get; set; }

        [IgnoreSensitiveWords]
        public string P4 { get; set; }

        public Test1 P5 { get; set; }
        public Test2 P6 { get; set; }

        public List<Test3> P7 { get; set; }

        public Dictionary<string, string> P8 { get; set; }

        public TestStruct P9 { get; set; }

        public class Test1
        {
            public string P1 { get; set; }

            public string P2 { get; set; }
        }

        public class Test2
        {
            public string P1 { get; set; }
            public Test Test { get; set; }
        }

        public class Test3
        {
            public string P1 { get; set; }

            public string P2 { get; set; }

            [SensitiveWords("SWTest")]
            public string P3 { get; set; }
        }

        public struct TestStruct
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
            public Test1 P3 { get; set; }
        }
    }
}