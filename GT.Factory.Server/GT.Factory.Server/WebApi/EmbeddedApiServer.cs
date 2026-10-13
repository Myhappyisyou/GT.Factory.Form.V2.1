using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.WebApi
{
    public class EmbeddedApiServer : IDisposable
    {
        private IDisposable _webServer;
        private readonly string _baseAddress;

        public EmbeddedApiServer(string baseAddress = "http://+:5000/")
        {
            _baseAddress = baseAddress;
        }

        /// <summary>启动内嵌 HTTP 服务</summary>
        public void Start()
        {
            _webServer = WebApp.Start<Startup>(_baseAddress);
            
            Console.WriteLine($"EmbeddedApiServer 已启动: {_baseAddress}");
        }

        public void Dispose()
        {
            _webServer?.Dispose();
        }
    }

}
