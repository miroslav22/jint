using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.IntegrationTests
{
    public class TestNetObject
    {
        public TestNetObject()
        {

        }

        public int TestProperty1 { get; set; } = 77;
        public static int StaticProperty1 { get; set; } = 64;
        public int TestMethod1() => 12;
        public int TestMethod2(int parm1, int parm2) => parm1 * parm2;

        public static int StaticMethod1() => 66;

        public string StringProperty => "hello";
        public bool BoolProperty => true;
        public object NullProperty => null;
        public object DoubleProperty => 55.5d;

        public Encoding getEncoding() => Encoding.UTF8;
    }
}
