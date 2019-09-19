using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net.Http;

namespace HttpClientFactoryPolly
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var fallbackResponse = new HttpResponseMessage();
            fallbackResponse.Content = new StringContent("fallback");
            fallbackResponse.StatusCode = System.Net.HttpStatusCode.TooManyRequests;
            // services.AddHttpClient();
            //重试
            //services.AddHttpClient("github", c =>
            //{
            //    //基址
            //    c.BaseAddress = new System.Uri("https://api.github.com/");
            //    // Github API versioning
            //    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //    // Github requires a user-agent
            //    c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            //    //AddTransientHttpErrorPolicy主要是处理Http请求的错误，如HTTP 5XX 的状态码，HTTP 408 的状态码 以及System.Net.Http.HttpRequestException异常
            //}).AddTransientHttpErrorPolicy(p =>
            ////WaitAndRetryAsync参数的意思是：每次重试时等待的睡眠持续时间。
            //p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));
            services.AddHttpClient("test", c =>
            {
                //基址
                c.BaseAddress = new System.Uri("http://localhost:5000/");
                // Github API versioning
                c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                // Github requires a user-agent
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            })
          //降级
          .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().FallbackAsync(fallbackResponse, async b =>
             {
                 Console.WriteLine($"fallback here {b.Exception.Message}");
             }))
            //熔断
            .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(4), (ex, ts) =>
            {
                Console.WriteLine($"break here {ts.TotalMilliseconds}");
            }, () =>
            {
                Console.WriteLine($"reset here ");
            }))
            //超时
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

                }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
