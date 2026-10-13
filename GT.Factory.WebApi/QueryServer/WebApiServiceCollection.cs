using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.WebApi.QueryServer
{
    //WebApi 服务注册表
    public class WebApiServiceCollection
    {
        private readonly Dictionary<Type, object> _services =
            new Dictionary<Type, object>();

        /// <summary>
        /// 注册 WebApi 所需要的外部服务。
        /// </summary>
        public void Add<TService>(TService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            _services[typeof(TService)] = service;
        }

        /// <summary>
        /// 获取已经注册的服务。
        /// </summary>
        public object Get(Type serviceType)
        {
            object service;

            if (_services.TryGetValue(serviceType, out service))
                return service;

            return null;
        }
    }
}
