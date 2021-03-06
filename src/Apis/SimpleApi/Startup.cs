using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleApi.Helpers;
using SimpleApi.Models;
using System;

namespace SimpleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationHelper.Instance.PropagateConfigurationAsync(configuration).Wait();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<HeroContext>(opt =>
                                     opt.UseInMemoryDatabase("TodoList"));

            services.AddControllers();

            services.AddDistributedMemoryCache();

            services.AddAuthentication("token")
            // reference tokens
            .AddOAuth2Introspection("token", options =>
            {
                options.Authority = ConfigurationHelper.Instance.Authority;
                options.EnableCaching = true;
                options.CacheDuration = TimeSpan.FromMinutes(15);
                options.SaveToken = true;

                options.ClientId = ConfigurationHelper.Instance.ClientId;
                options.ClientSecret = ConfigurationHelper.Instance.ClientSecret;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AccessTokenHandler>();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<HeroContext>();
                AddSeedingData(context);
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization("ApiScope");
            });
        }

        private void AddSeedingData(HeroContext context)
        {
            context.Database.EnsureCreated();
            context.Heroes.Add(new Hero() { Id = 11, Name = "Dr Nice" });
            context.Heroes.Add(new Hero() { Id = 12, Name = "Narco" });
            context.Heroes.Add(new Hero() { Id = 13, Name = "Bombasto" });
            context.Heroes.Add(new Hero() { Id = 14, Name = "Celeritas" });
            context.Heroes.Add(new Hero() { Id = 15, Name = "Magneta" });
            context.Heroes.Add(new Hero() { Id = 16, Name = "RubberMan" });
            context.Heroes.Add(new Hero() { Id = 17, Name = "Dynama" });
            context.Heroes.Add(new Hero() { Id = 18, Name = "Dr IQ" });
            context.Heroes.Add(new Hero() { Id = 19, Name = "Magma" });
            context.Heroes.Add(new Hero() { Id = 20, Name = "Tornado" });
            context.SaveChanges();
        }
    }
}
