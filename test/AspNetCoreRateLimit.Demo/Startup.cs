using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Demo
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
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //configure ip rate limiting middle-ware
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            //configure client rate limiting middleware
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            //services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            var opt = new ClientRateLimitOptions();
            ConfigurationBinder.Bind(Configuration.GetSection("ClientRateLimiting"), opt);

            services.AddMvc().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIpRateLimiting();
            app.UseClientRateLimiting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}