using Astrarium.Types;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Newtonsoft.Json.Converters;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Astrarium.Plugins.RestAPI
{
    public interface IRestAPIServer
    {
        void Start();
        void Stop();
        string State { get; }
        bool IsStarted { get; }
    }

    [Singleton(typeof(IRestAPIServer))]
    public class RestAPIServer : PropertyChangedBase, IRestAPIServer, IAssembliesResolver, IHttpControllerActivator, IExceptionHandler
    {
        private IDisposable webApplication;
        private CancellationTokenSource cancellationTokenSource;
        private Task serverTask;

        private readonly IRestAPIController controller;
        private readonly ISettings settings;

        public RestAPIServer(IRestAPIController controller, ISettings settings)
        {
            this.controller = controller;
            this.settings = settings;

            State = Text.Get("RestAPIServerState.Stopped");
        }

        public string State
        {
            get => GetValue<string>(nameof(State));
            private set => SetValue(nameof(State), value);
        }

        public bool IsStarted
        {
            get => GetValue<bool>(nameof(IsStarted));
            private set => SetValue(nameof(IsStarted), value);
        }

        public void Start()
        {
            string baseAddress = Utils.GetRestAPIServerBaseAddress(settings);

            cancellationTokenSource = new CancellationTokenSource();

            serverTask = Task.Run(() =>
            {
                try
                {
                    Log.Info($"Starting REST API server...");
                    State = Text.Get("RestAPIServerState.Starting");
                    webApplication = WebApp.Start(url: baseAddress, AppBuilder);
                    Log.Info($"REST API server started.");
                    State = Text.Get("RestAPIServerState.Started");
                    IsStarted = true;
                    cancellationTokenSource.Token.WaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    var message = ex.GetBaseException().Message;
                    Log.Error($"REST API server error: {message}");
                    State = $"{Text.Get("RestAPIServerState.Failed")}: {message}";
                    IsStarted = false;
                }
            });
        }

        public void Stop()
        {
            Log.Info($"Stopping REST API server...");
            State = Text.Get("RestAPIServerState.Stopping");
            cancellationTokenSource?.Cancel();
            webApplication?.Dispose();
            serverTask?.Wait(TimeSpan.FromSeconds(5));
            Log.Info($"REST API server stopped.");
            State = Text.Get("RestAPIServerState.Stopped");
            IsStarted = false;
        }

        private void AppBuilder(IAppBuilder appBuilder)
        {
            appBuilder.Use(async (context, next) =>
            {
                if (context.Request.Uri.LocalPath.StartsWith("/api/"))
                {
                    Log.Debug($"REST API request: {context.Request.Method} {context.Request.Uri}");
                }
                await next();
            });

            

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var swaggerUiPath = Path.Combine(baseDirectory, "SwaggerUI");

            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), this);
            config.Services.Replace(typeof(IHttpControllerActivator), this);
            config.Services.Replace(typeof(IExceptionHandler), this);

            config.MapHttpAttributeRoutes();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
     
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;

            jsonSettings.Converters.Add(new StringEnumConverter());
            jsonSettings.Converters.Add(new DateJsonConverter());

            config.EnsureInitialized();

            var fileServerOptions = new FileServerOptions
            {
                FileSystem = new PhysicalFileSystem(swaggerUiPath),
                RequestPath = new PathString("/swagger"),
            };

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings.Add(".yaml", "application/x-yaml");
            fileServerOptions.StaticFileOptions.ContentTypeProvider = contentTypeProvider;

            appBuilder.UseWebApi(config);
            appBuilder.UseFileServer(fileServerOptions);
            
        }

        ICollection<Assembly> IAssembliesResolver.GetAssemblies()
        {
            return new[] { Assembly.GetExecutingAssembly() };
        }

        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return new RestAPIController(controller);
        }

        Task IExceptionHandler.HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var error = context.Exception;
                HttpStatusCode statusCode = error is BaseError restError ? restError.StatusCode : HttpStatusCode.InternalServerError;

                context.Result = new ResponseMessageResult(context.Request.CreateResponse(statusCode, new
                {
                    error = error.GetType().Name,
                    message = error.Message,
                }));
            });
        }
    }
}
