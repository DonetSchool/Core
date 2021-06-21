using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QosTest.Controllers;
using System;
using System.IO;
using System.Net;

namespace QosTest
{
    public class Startup
    {
        public Startup()
        {
            Configuration =
                       new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .Build();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddQoS(Configuration, configure: (builder) =>
            //{
            //    builder.OnLimitProcessResult += (requestIdentity, quotaConfig, isAllow, waittimeMills) =>
            //    {
            //        ServiceProvider.GetRequiredService<ILogger<Startup>>().LogDebug("requestIdentity->{0},isAllow->{1},waittimeMills->{2}", requestIdentity, isAllow, waittimeMills);
            //    };
            //    builder.OnFallbackAction += async (exception, keyValuePairs, cancellationToken) =>
            //    {
            //        var requestDelegate = keyValuePairs["RequestDelegate"] as RequestDelegate;
            //        var httpContext = keyValuePairs["HttpContext"] as HttpContext;
            //        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            //        await httpContext.Response.WriteAsync("Request too many");
            //    };
            //});
            services.AddControllers()
                ;
            services.AddZk(Configuration).AddZkListener<TestMemoryWatch>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServiceProvider = app.ApplicationServices;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseQoS();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}