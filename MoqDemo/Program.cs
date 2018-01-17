using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoqDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var mock = new Mock<IFoo>();

            //1.参数为固定值"ping"时候返回true。
            mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
            Console.WriteLine(mock.Object.DoSomething("ping"));

            // 2.out 参数
            // TryParse方法将返回true,并且out 参数将返回"ACK", lazy evaluated
            var outString = "ACK";
            string print = string.Empty;
            mock.Setup(foo => foo.TryParse("ping", out outString)).Returns(true);
            Console.WriteLine(mock.Object.TryParse("ping",out print) + "  print:" + print);

            // 3.ref 参数
            // 当应用参数为同一实例时返回reture
            var instance = new Bar();
            var instance2 = new Bar();
            var instance3 = instance;
            mock.Setup(foo => foo.Submit(ref instance)).Returns(true);
            Console.WriteLine(mock.Object.Submit(ref instance2) + "  " + instance);

            // 4.access invocation arguments when returning a value
            mock.Setup(x => x.DoSomethingStringy(It.IsAny<string>()))
                    .Returns((string s) => s.ToLower());
            // Multiple parameters overloads available
            Console.WriteLine(mock.Object.DoSomethingStringy("UPCASECONVERT "));

            // throwing when invoked with specific parameters
            mock.Setup(foo => foo.DoSomething("reset")).Throws<InvalidOperationException>();
            mock.Setup(foo => foo.DoSomething("")).Throws(new ArgumentException("command"));
            //mock.Object.DoSomething("");
            mock.Object.DoSomething("notreset");

            //5. lazy evaluating return value
            var count = 1;
            mock.Setup(foo => foo.GetCount()).Returns(() => count);
            Console.WriteLine(mock.Object.GetCount());

            //6. returning different values on each invocation
            var countWithCallback = 1;
            mock.Setup(foo => foo.GetCount()).Returns(() => countWithCallback).Callback(() => countWithCallback++);
            Console.WriteLine( "Once:"+ mock.Object.GetCount() + " Twice:" +mock.Object.GetCount() + " Thrid:"+ mock.Object.GetCount());

            #region  Matching Arguments
            /*
             *  any value
             *  mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
             *  
             *  any value passed in a `ref` parameter (requires Moq 4.8 or later):
             *  mock.Setup(foo => foo.Submit(ref It.Ref<Bar>.IsAny)).Returns(true);
             *  
             *  matching Func<int>, lazy evaluated
             *  mock.Setup(foo => foo.Add(It.Is<int>(i => i % 2 == 0))).Returns(true); 
             *  
             *  matching ranges
             *  mock.Setup(foo => foo.Add(It.IsInRange<int>(0, 10, Range.Inclusive))).Returns(true); 
             *  
             *  matching regex
             *  mock.Setup(x => x.DoSomethingStringy(It.IsRegex("[a-d]+", RegexOptions.IgnoreCase))).Returns("foo");
             */
            #endregion



            Console.ReadKey();
}
}
}
