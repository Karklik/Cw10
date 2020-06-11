using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CW10.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                var path = context.Request.Path;
                var method = context.Request.Method;
                var queryString = context.Request.QueryString;
                var body = "";
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                using var writer = new StreamWriter("requestsLog.txt", true);
                writer.WriteLine("====================================================================");
                writer.WriteLine("On: " + DateTime.Now.ToString());
                writer.WriteLine("Method: " + method);
                writer.WriteLine("Path: " + path);
                writer.WriteLine("QueryString: " + queryString);
                writer.WriteLine("Body:\n" + body);
            }

            await _next(context);
        }
    }
}
