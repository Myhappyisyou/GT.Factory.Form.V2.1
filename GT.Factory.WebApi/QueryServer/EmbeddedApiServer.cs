using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.WebApi.QueryServer
{
    public class EmbeddedApiServer : IDisposable
    {
        private IDisposable _webServer;
        private readonly string _baseAddress;
        private readonly WebApiServiceCollection _services;

        public EmbeddedApiServer(
            WebApiServiceCollection services,
            string baseAddress = "http://+:5000/")
        {
            if (services == null)
                throw new ArgumentNullException("services");

            _services = services;

            _baseAddress = baseAddress;
        }

        public void Start()
        {
            _webServer = WebApp.Start(
                _baseAddress,
                delegate (IAppBuilder app)
                {
                    Startup startup =
                        new Startup(_services);

                    startup.Configuration(app);
                });

            Console.WriteLine(
                "EmbeddedApiServer 已启动: " + _baseAddress);
        }

        public void Dispose()
        {
            if (_webServer != null)
            {
                _webServer.Dispose();
                _webServer = null;
            }
        }

        /// <summary>启动内嵌 HTTP 服务</summary>
        //public void Start()
        //{
        //    _webServer = WebApp.Start<Startup>(_baseAddress);
            
        //    Console.WriteLine($"EmbeddedApiServer 已启动: {_baseAddress}");
        //}

        //public void Dispose()
        //{
        //    _webServer?.Dispose();
        //}
    }

}
