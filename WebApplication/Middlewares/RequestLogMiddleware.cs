using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using System.Text;
using WebApplication.Helpers;

namespace WebApplication.Middlewares
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public RequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await CreateLogAsync(httpContext);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private async Task CreateLogAsync(HttpContext context)
        {
            var controllerActionDescriptor = context.GetEndpoint()?.Metadata?.GetMetadata<ControllerActionDescriptor>();

            HTTPLog log = new HTTPLog();

            try
            {
                log.RequestTime = DateTime.Now;
                log.Controller = controllerActionDescriptor?.ControllerName;
                log.Action = controllerActionDescriptor?.ActionName;
                log.EndPoint = context.Request.Path;
                log.QueryString = $"{context.Request.QueryString}";
                log.MethodType = context.Request.Method;
                log.CallerIp = context.Connection.RemoteIpAddress.ToString() + ":" + context.Connection.RemotePort;
                log.Token = "Token";
                log.RequestMessage = await ReadRequestBody(context.Request);
                log.RequestHeader = JsonConvert.SerializeObject(
                    context.Request.Headers,
                    Formatting.Indented,
                    new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }
                );
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }


            var responseBody = new MemoryStream(); //Create a new memory stream...
            var originalBodyStream = context.Response.Body; //Copy a pointer to the original response body stream
            try
            {
                context.Response.Body = responseBody; //...and use that for the temporary response body
                await _next(context); //Continue down the Middleware pipeline, eventually returning to this class
                log.ResponseTime = DateTime.Now;
                log.StatusCode = context.Response.StatusCode;
                log.ResponseHeader = JsonConvert.SerializeObject(
                    context.Response.Headers,
                    Formatting.Indented,
                    new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }
                );
                log.ResponseMessage = await ReadResponseBody(context.Response);
                log.UserIdentity = "Fatih";
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            await responseBody.CopyToAsync(originalBodyStream);

            WriteLog(log);
        }

        public void WriteLog(HTTPLog log)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Endpoint : {0}");
            stringBuilder.AppendLine("Controller: {1}");
            stringBuilder.AppendLine("Action: {2}");
            stringBuilder.AppendLine("Method: {3}");
            stringBuilder.AppendLine("Status Code: {4}");
            stringBuilder.AppendLine("Query String: {5}");
            stringBuilder.AppendLine("Caller IP: {6}");
            stringBuilder.AppendLine("Token: {7}");
            stringBuilder.AppendLine("User Identity: {8}");
            stringBuilder.AppendLine("Request Time : {9:dd/MM/yyyy HH:mm:ss [yyyy-MM-dd HH:mm:ss]}");
            stringBuilder.AppendLine("Request Message : {10}");
            stringBuilder.AppendLine("Response Time: {11:dd/MM/yyyy HH:mm:ss [yyyy-MM-dd HH:mm:ss]}");
            stringBuilder.AppendLine("Response Message : {12}");
            stringBuilder.AppendLine("Request Headers : {13}");
            stringBuilder.AppendLine("Response Headers : {14}");

            string logText = LogDrawer.Draw(
                string.Format(
                    stringBuilder.ToString(),
                    log.EndPoint,
                    log.Controller,
                    log.Action,
                    log.MethodType,
                    log.StatusCode,
                    log.QueryString,
                    log.CallerIp,
                    log.Token,
                    log.UserIdentity,
                    log.RequestTime,
                    log.RequestMessage,
                    log.ResponseTime,
                    log.ResponseMessage,
                    log.RequestHeader,
                    log.ResponseHeader
                )
            );

            Logger.Info(logText);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            request.EnableBuffering();

            // Leave the body open so the next middleware can read it.
            using var reader = new StreamReader(request.Body, Encoding.UTF8, false, Convert.ToInt32(request.ContentLength), true);
            var body = await reader.ReadToEndAsync();
            // Do some processing with body…

            // Reset the request body stream position so the next middleware can read it
            request.Body.Position = 0;

            return body;
        }
        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            string text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response
            return text;
        }
    }
    public static class RequestLogMiddlewareExtensions
    {
        public static IApplicationBuilder RequestLogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogMiddleware>();
        }
    }
    public class HTTPLog
    {
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public string Token { get; set; }
        public string UserIdentity { get; set; }
        public string EndPoint { get; set; }
        public string QueryString { get; set; }
        public string MethodType { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseHeader { get; set; }
        public string CallerIp { get; set; }
        public string ResponseMessage { get; set; }
        public string RequestMessage { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}