using Austin.LibTaskNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockServer
{
    public class MockServer
    {
        private static RequestResolver Resolver { get; } = new RequestResolver();

        public static void StartServer()
          => StartServerAt("8080");

        public static void StartServerAt(string port)
        {
            CoopScheduler.AddTask(HttpServer);
            CoopScheduler.StartScheduler();
        }

        public static void AddController<T>() where T : new()
           => Resolver.Add<T>();

        private static async Task HttpServer()
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
                CoopScheduler.AddTask(() => HandleRequest(ctx));
            }
        }

        private static async Task HandleRequest(HttpListenerContext ctx) => await Resolver.Resolve(ctx, null);

       
    }
}
