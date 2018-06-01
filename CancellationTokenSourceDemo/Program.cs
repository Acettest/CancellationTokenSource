using System;
using System.Threading;

namespace CancellationTokenSourceDemo
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            //技巧：匿名方法的使用，使用线程池传入不符合委托形式的函数，第二个参数可以传入一个强类型的参数
            ThreadPool.QueueUserWorkItem(o => WriteOperationOne(cts.Token, null));//支持取消
            //ThreadPool.QueueUserWorkItem(o => WriteOperationTwo(CancellationToken.None, null));//不支持取消
            ThreadPool.QueueUserWorkItem(o => WriteOperationThree(cts.Token, 1000));//不支持取消
            //ThreadPool.QueueUserWorkItem(o => WriteOperationThree(CancellationToken.None, 2000));//不支持取消
            //注册执行的顺序与注册的顺序相反，这是为什么呢？
            cts.Token.Register(() => { Console.WriteLine("取消完成！"); });
            cts.Token.Register(() => { Console.WriteLine("开始取消！"); });
            var ctr = cts.Token.Register(() => { Console.WriteLine("取消操作3！"); });

            //登记回调删除，这样当调用cts.Cancel()时便不会发生调用该回调
            ctr.Dispose();

            while (true)
            {
                Console.WriteLine("输入Enter取消");
                if (Console.ReadKey().Key.Equals(ConsoleKey.Enter))
                {
                    cts.Cancel();//即时取消
                    //cts.CancelAfter(2000);
                    break;
                }
            }
            Console.WriteLine("取消将在两秒后完成");
            Console.WriteLine("休眠2S");
            Thread.Sleep(2000);
            var cts1 = new CancellationTokenSource();
            cts1.Token.Register(() => Console.WriteLine("cts1取消"));
            var cts2 = new CancellationTokenSource();
            cts2.Token.Register(() => Console.WriteLine("cts2取消"));
            var ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);
            cts2.CancelAfter(2000);//延迟取消
            Console.WriteLine("cts1 canceled: " + cts1.IsCancellationRequested + ". cts2 canceled: " + cts2.IsCancellationRequested + ". ctsLinked canceld:" + ctsLinked.IsCancellationRequested);
            Thread.Sleep(2000);
            Console.WriteLine("休眠2S");
            Console.WriteLine("cts1 canceled: " + cts1.IsCancellationRequested + ". cts2 canceled: " + cts2.IsCancellationRequested + ". ctsLinked canceld:" + ctsLinked.IsCancellationRequested);

            Console.ReadLine();
        }

        private static void WriteOperationOne(CancellationToken token, object state)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                Console.WriteLine("操作请求1：" + DateTime.Now.ToLocalTime());
                Thread.Sleep(500);//出于演示目的，此处造成阻塞，浪费一些性能,下同
            }
        }

        private static void WriteOperationThree(CancellationToken token, Int32 count)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                Console.WriteLine("操作请求3：" + count.ToString());
                Thread.Sleep(500);
            }
        }

        private static void WriteOperationTwo(CancellationToken token, object state)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                Console.WriteLine("操作请求2：" + DateTime.Now.ToLocalTime());
                Thread.Sleep(500);
            }
        }
    }
}