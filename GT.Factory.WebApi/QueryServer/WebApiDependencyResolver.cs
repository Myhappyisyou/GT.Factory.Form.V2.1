using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace GT.Factory.WebApi.QueryServer
{
    public class WebApiDependencyResolver : IDependencyResolver
    {
        private readonly WebApiServiceCollection _services;

        public WebApiDependencyResolver(
            WebApiServiceCollection services)
        {
            _services = services;
        }

        public object GetService(Type serviceType)
        {
            return _services.Get(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new object[0];
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
