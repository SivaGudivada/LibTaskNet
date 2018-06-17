using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PathToMethodMapAttribute : Attribute
    {
        public string Route { get; set; } = "/";
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

    public class RequestResolver
    {

        private Dictionary<Type, object> controllers = new Dictionary<Type, object>();

        public void Add<T>() where T : new()
        {
            if (!controllers.ContainsKey(typeof(T)))
            {
                controllers.Add(typeof(T), new T());
            }
        }

        public async Task Resolve(HttpListenerContext context, object data)
        {
            string path = context.Request.Url.AbsolutePath.ToString();
            string method = context.Request.HttpMethod;

            Console.WriteLine($"Received a {method} request. Path : {path}");

            Predicate<PathToMethodMapAttribute> mapPredicate = p =>
                p.Route.Equals(path) && p.Method.Equals(method);


            await Task.Run(() =>
                {
                    foreach (Type item in controllers.Keys)
                    {
                        var methodInf = controllers[item]
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
                            methodInf.Invoke(Activator.CreateInstance(item), new object[] { context, data });
                        }
                    }
                });

        }
    }
}
