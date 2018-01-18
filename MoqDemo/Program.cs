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
            // TryParse方法将返回true,并且out 参数将返回"ACK", lazy evaluated(惰性求值)
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
            Console.WriteLine(mock.Object.Submit(ref instance2));

            // 4.在返回值是访问调用参数
            mock.Setup(x => x.DoSomethingStringy(It.IsAny<string>()))
                    .Returns((string s) => s.ToLower());
            // Multiple parameters overloads available
            Console.WriteLine(mock.Object.DoSomethingStringy("UPCASECONVERT "));

            // 当调用传入特定参数时抛出异常
            mock.Setup(foo => foo.DoSomething("reset")).Throws<InvalidOperationException>();
            mock.Setup(foo => foo.DoSomething("")).Throws(new ArgumentException("command"));
            //mock.Object.DoSomething("");
            mock.Object.DoSomething("notreset");

            //5. lazy evaluating return value
            var count = 1;
            mock.Setup(foo => foo.GetCount()).Returns(() => count);
            Console.WriteLine(mock.Object.GetCount());

            //6.当每次调用时返回不同的值
            var countWithCallback = 1;
            mock.Setup(foo => foo.GetCount()).Returns(() => countWithCallback).Callback(() => countWithCallback++);
            Console.WriteLine( "Once:"+ mock.Object.GetCount() + " Twice:" +mock.Object.GetCount() + " Thrid:"+ mock.Object.GetCount());

            #region  Matching Arguments 参数匹配
            /*
             *  any value - It.IsAny<T>
             *  mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
             *  
             *  any value passed in a `ref` parameter (requires Moq 4.8 or later):
             *  mock.Setup(foo => foo.Submit(ref It.Ref<Bar>.IsAny)).Returns(true);
             *  
             *  matching Func<int>, lazy evaluated - It.Is<int>(Func<T>)
             *  mock.Setup(foo => foo.Add(It.Is<int>(i => i % 2 == 0))).Returns(true); 
             *  
             *  matching ranges - It.IsInRange<T>(S,E,Range)  [Range means opened/closed interval]
             *  mock.Setup(foo => foo.Add(It.IsInRange<int>(0, 10, Range.Inclusive))).Returns(true); 
             *  
             *  matching regex It.IsRegex(regexString,i)
             *  mock.Setup(x => x.DoSomethingStringy(It.IsRegex("[a-d]+", RegexOptions.IgnoreCase))).Returns("foo");
             */
            #endregion

            #region //Properties

            //7.Get property value
            mock.Setup(foo => foo.Name).Returns("Name");
            Console.WriteLine($"Name property = {mock.Object.Name } ");

            //8. auto-mocking hierarchies (a.k.a. recursive mocks)
            mock.Setup(foo => foo.Bar.Baz.Name).Returns("baz");
            Console.WriteLine($"Bar.Baz.Name property = {mock.Object.Bar.Baz.Name } ");

            // expects an invocation to set the value to "foo"  ???
            //mock.SetupSet(foo => foo.Name = "foo");

            // or verify the setter directly ???
            //mock.VerifySet(foo => foo.Name = "foo");


            //Setup a property so that it will automatically start tracking its value(also known as Stub):
            var mocks = new Mock<IFoo>();
            IFoo foos = mocks.Object;
            // start "tracking" sets/gets to this property
            mocks.SetupProperty(f => f.Name); //foos.Name=null
            // alternatively, provide a default value for the stubbed property
            mocks.SetupProperty(f => f.Name, "foo");  //foos.name="foo"
            // Now you can do:
            
            // Initial value was stored
            Console.WriteLine($"foo == foos.Name:{ "foo" == foos.Name }");
            // New value set which changes the initial value
            foos.Name = "bar";
            Console.WriteLine($"bar == foos.Name:{ "bar"== foos.Name }");
            //Stub all properties on a mock (not available on Silverlight):
            /*指定模拟上的所有属性应该具有“属性行为”，这意味着设置其值将导致它被保存，并且在请求属性时返回。
            （这也被称为“桩”）。 每个属性的默认值将是由模拟的Moq.Mock.DefaultValue属性指定的值。*/
            mock.SetupAllProperties();
            Console.WriteLine($"{mock.Object.Value}");
            #endregion

            #region //Event

            /*
                 // Raising an event on the mock
                mock.Raise(m => m.FooEvent += null, new FooEventArgs(fooValue));

                // Raising an event on a descendant down the hierarchy
                mock.Raise(m => m.Child.First.FooEvent += null, new FooEventArgs(fooValue));

                // Causing an event to raise automatically when Submit is invoked
                mock.Setup(foo => foo.Submit()).Raises(f => f.Sent += null, EventArgs.Empty);
                // The raised event would trigger behavior on the object under test, which 
                // you would make assertions about later (how its state changed as a consequence, typically)

                // Raising a custom event which does not adhere to the EventHandler pattern
                public delegate void MyEventHandler(int i, bool b);
                public interface IFoo
                {
                event MyEventHandler MyEvent; 
                }

                var mock = new Mock<IFoo>();
                ...
                // Raise passing the custom arguments expected by the event delegate
                mock.Raise(foo => foo.MyEvent += null, 25, true);
             */

            #endregion

            #region Callbacks

            mock = new Mock<IFoo>();
            var calls = 0;
            var callArgs = new List<string>();

            mock.Setup(foo => foo.DoSomething("ping"))
                .Returns(true)
                .Callback(() => calls++);

            // access invocation arguments
            mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
                    .Returns(true).Callback( new Action<string>(s => callArgs.Add(s)));

            // alternate equivalent generic method syntax
            //mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
            //    .Returns(true)
            //    .Callback<string>(s => callArgs.Add(s));

            // access arguments for methods with multiple parameters
            //mock.Setup(foo => foo.DoSomething(It.IsAny<int>(), It.IsAny<string>()))
            //    .Returns(true)
            //    .Callback<int, string>((i, s) => callArgs.Add(s));

            // callbacks can be specified before and after invocation
            mock.Setup(foo => foo.DoSomething("ping"))
                .Callback(() => Console.WriteLine("Before returns"))
                .Returns(true)
                .Callback(() => Console.WriteLine("After returns"));
            mock.Object.DoSomething("ping");

            // callbacks for methods with `ref` / `out` parameters are possible but require some work (and Moq 4.8 or later):
            //delegate void SubmitCallback(ref Bar bar);

            //mock.Setup(foo => foo.Submit(ref It.Ref<Bar>.IsAny)
            //    .Callback(new SubmitCallback((ref Bar bar) => Console.WriteLine("Submitting a Bar!"));

            #endregion

            Console.ReadKey();
        }
    }
}
