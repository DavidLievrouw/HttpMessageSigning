using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Dalion.HttpMessageSigning.Verification.Owin;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;

[assembly: OwinStartup(typeof(OwinApplication.Startup))]

namespace OwinApplication {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            var config = new HttpConfiguration {
                DependencyResolver = new DefaultDependencyResolver(IocStartup.BuildServiceProvider())
            };

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
            );

            var defaultSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> {
                    new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()}
                }
            };
            JsonConvert.DefaultSettings = () => defaultSettings;
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = defaultSettings;

            app
                .UseHttpRequestSignatureAuthentication(new SignedHttpRequestAuthenticationOptions {
                    Realm = "Sample OWIN application",
                    RequestSignatureVerifier = (IRequestSignatureVerifier) config.DependencyResolver.GetService(typeof(IRequestSignatureVerifier)),
                    OnIdentityVerified = successResult => {
                        var identity = (ClaimsIdentity) successResult.Principal.Identity;
                        Console.WriteLine("Identity '{0}' was authenticated by the request signature.", identity.Name ?? "[NULL]");
                        return Task.CompletedTask;
                    },
                    OnIdentityVerificationFailed = failure => {
                        Console.WriteLine("The request signature could not be verified. Authentication failed: {0}", failure.Failure.Message);
                        return Task.CompletedTask;
                    }
                })
                .UseWebApi(config);
        }
    }
}