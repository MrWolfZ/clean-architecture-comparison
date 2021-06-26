using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.Core.TestUtilities
{
    public abstract class CoreIntegrationTestBase
    {
        private HttpClient? client;
        private IHost? host;

        protected HttpClient HttpClient
        {
            get
            {
                if (client == null)
                {
                    throw new InvalidOperationException("test fixture must be initialized before using http client");
                }

                return client;
            }
        }

        protected IHost Host
        {
            get
            {
                if (host == null)
                {
                    throw new InvalidOperationException("test fixture must be initialized before using host");
                }

                return host;
            }
        }

        protected virtual string ApiPrefix => "api";

        protected JsonSerializerOptions JsonSerializerOptions => Resolve<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

        [SetUp]
        public async Task SetUp()
        {
            var hostBuilder = new HostBuilder().ConfigureWebHost(webHost =>
            {
                _ = webHost.UseTestServer();
                
                ConfigureWebHost(webHost);

                _ = webHost.ConfigureServices(ConfigureServices);
            });

            host = await hostBuilder.StartAsync();
            client = host.GetTestClient();

            client.BaseAddress = new(client.BaseAddress!, $"{ApiPrefix}/");
        }

        [TearDown]
        public void TearDown()
        {
            host?.Dispose();
            HttpClient.Dispose();
        }

        protected abstract void ConfigureWebHost(IWebHostBuilder webHost);

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        protected T Resolve<T>()
            where T : notnull => Host.Services.GetRequiredService<T>();
    }
}
