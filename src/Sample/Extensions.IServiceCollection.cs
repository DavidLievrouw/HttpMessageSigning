using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sample {
    public static class Extensions {
        public static IServiceCollection Configure(this IServiceCollection services, Action<IServiceCollection> configure) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            configure(services);
            return services;
        }

        public static IServiceCollection Configure(this IServiceCollection services, Func<IServiceCollection, IServiceCollection> configure) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return configure(services);
        }
    }
}