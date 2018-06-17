using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // MockServer usage example
            MockServer.AddController<TestController>();
            MockServer.StartServer();
        }
        public class TestController
        {
            // TODO: Add controller attribute
            public TestController()
            {

            }
            [PathToMethodMap(Route = "/test", Method = "GET")]
            async public Task Get(System.Net.HttpListenerContext context, object data)
            => await context.WriteToResponse("Executed 'Get' in test controller");

            [PathToMethodMap(Route = "/test", Method = "POST")]
            async public Task Post(System.Net.HttpListenerContext context, object data)
           => await context.WriteToResponse("Executed 'Post' in test controller");
        }
    }
}
