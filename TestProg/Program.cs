using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Austin.LibTaskNet;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TestProg
{
    class Program
    {
        private static async Task handleRequest(HttpListenerContext ctx)
        {
            RequestResolver.Resolve (ctx,null);
        }

        private static async Task httpServer()
        {
            CoopScheduler.AddTask(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("waiting");
                }
            });

            HttpListener l = new HttpListener();
            l.Prefixes.Add("http://+:8080/");
            l.Start();
            while (true)
            {
                var ctx = await l.GetContextAsync();
                CoopScheduler.AddTask(() => handleRequest(ctx));
            }
        }

        static void Main(string[] args)
        {
            CoopScheduler.AddTask(httpServer);
            CoopScheduler.StartScheduler();
        }
    }
}
