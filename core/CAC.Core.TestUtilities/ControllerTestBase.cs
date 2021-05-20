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
    public abstract class ControllerTestBase
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
                webHost.UseTestServer();

                ConfigureWebHost(webHost);
            });

            host = await hostBuilder.StartAsync();
            client = host.GetTestClient();

            client.BaseAddress = new Uri(client.BaseAddress!, $"{ApiPrefix}/");
        }

        [TearDown]
        public void TearDown()
        {
            host?.Dispose();
            HttpClient.Dispose();
        }

        protected abstract void ConfigureWebHost(IWebHostBuilder webHost);

        protected string Serialize(object o) => JsonSerializer.Serialize(o, JsonSerializerOptions);

        protected T? Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, JsonSerializerOptions);

        protected T Resolve<T>()
            where T : notnull => Host.Services.GetRequiredService<T>();
    }
}
