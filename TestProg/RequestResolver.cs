using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProg
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PathToMethodMapAttribute : Attribute
    {
        public string PathToMapTo { get; set; }
        public string Method { get; set; } = "GET";
    }
    public static class ContextExtensions
    {
         public static void Send404(this HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
        }


        async public static Task WriteToResponse(this HttpListenerContext context, string responseString)
        {
            context.SetResponseContentType("text/plain");
            var os = context.Response.OutputStream;
            var bytes = responseString.ToASCIIBytes();
            await os.WriteAsync(bytes, 0, bytes.Length);
            context.Response.Close();
        }

        public static void SetResponseContentType(this HttpListenerContext context, string contentType)
            => context.Response.ContentType = contentType;

        public static byte[] ToASCIIBytes(this string str) 
            => Encoding.ASCII.GetBytes(str);

    }

    public class TestController
    {
        // TODO: Add controller attribute
        public TestController()
        {

        }
        [PathToMethodMap(PathToMapTo = "/test", Method = "GET")]
        async public Task Get(HttpListenerContext context, object data)
        => await context.WriteToResponse("Executed 'Get' in test controller");

        [PathToMethodMap(PathToMapTo = "/test", Method = "POST")]
        async public Task Post(HttpListenerContext context, object data)
       => await context.WriteToResponse("Executed 'Post' in test controller");
    }

    public class RequestResolver
    {
        // TODO : Add DI ? 

        static Dictionary<Type, object> controllers = new Dictionary<Type, object>();

        public static void Add<T>() where T : new()
        {
            controllers.Add(typeof(T), new T());
        }
        
        public static void Resolve(HttpListenerContext context, object data)
        {
            Predicate<PathToMethodMapAttribute> mapPredicate = 
                p => p.PathToMapTo.Equals(context.Request.Url.AbsolutePath.ToString()) && 
                     context.Request.HttpMethod.Equals(p.Method);
            
            // TODO : Get Instance from DI container?

            var controllerObj = new TestController();

            var methodInf = controllerObj
                .GetType()
                .GetMethods()
                .FirstOrDefault(
                    m => m.GetCustomAttributes(typeof(PathToMethodMapAttribute), false)?
                    .FirstOrDefault(
                        x => mapPredicate((PathToMethodMapAttribute)x)) != null);
            if (methodInf == null)
            {
                context.Send404();
            }
            else
            {
                methodInf.Invoke(controllerObj, new object[] { context, data });
            }
        }
    }
}
