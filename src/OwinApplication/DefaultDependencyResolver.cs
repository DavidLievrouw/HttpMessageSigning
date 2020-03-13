using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace OwinApplication {
    public class DefaultDependencyResolver : IDependencyResolver {
        private readonly IServiceProvider _provider;

        public DefaultDependencyResolver(IServiceProvider provider) {
            _provider = provider;
        }

        public object GetService(Type serviceType) {
            return _provider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType) {
            return _provider.GetServices(serviceType);
        }

        public IDependencyScope BeginScope() {
            return this;
        }

        public void Dispose() { }
    }
}