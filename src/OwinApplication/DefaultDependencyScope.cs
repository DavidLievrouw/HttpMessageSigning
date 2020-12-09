using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace OwinApplication {
    public class DefaultDependencyScope : IDependencyScope {
        private readonly IServiceScope _scope;

        public DefaultDependencyScope(IServiceScope scope) {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public void Dispose() {
            _scope.Dispose();
        }

        public object GetService(Type serviceType) {
            return _scope.ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType) {
            return _scope.ServiceProvider.GetServices(serviceType);
        }
    }
}