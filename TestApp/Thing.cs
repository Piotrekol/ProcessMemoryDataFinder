using System.Collections.Generic;

namespace TestApp
{
    public class Thing
    {
        public long Pattern { get; set; }
        public int Int { get; set; }
        public int[] IntArr { get; set; }
        public List<int> IntList { get; set; }
        public string String { get; set; }

        public Thing(long lMin2000)
        {
            Pattern = lMin2000 + 2000;
            Int = 0;
            IntArr = null;
            IntList = null;
            String = "I Am A String";
        }
    }
}