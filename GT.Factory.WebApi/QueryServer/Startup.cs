using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace GT.Factory.WebApi.QueryServer
{

    public class Startup
    {
        private readonly WebApiServiceCollection _services;

        public Startup(WebApiServiceCollection services)
        {
            _services = services;
        }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.DependencyResolver =
                new WebApiDependencyResolver(_services);

            app.UseWebApi(config);
        }

        //public void Configuration(IAppBuilder app)
        //{
        //    var config = new HttpConfiguration();
        //    config.MapHttpAttributeRoutes();
        //    app.UseWebApi(config);
        //}
    }

}
