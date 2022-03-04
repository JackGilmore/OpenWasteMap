using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Repositories;
using Serilog;

namespace OpenWasteMapUK
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _environment = env;
        }

        private readonly IWebHostEnvironment _environment;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source = database.db"), ServiceLifetime.Transient);

            //if (_environment.IsProduction() && Configuration["WEBSITE_HOSTNAME"].Contains("azurewebsites"))
            //{
            //    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["POSTGRESQLCONNSTR_Default"]), ServiceLifetime.Transient);
            //}
            //else
            //{
            //    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")), ServiceLifetime.Transient);
            //}

            services.AddScoped<IDataRepository, DataRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();


                if (Configuration["AUTO_MIGRATE"] == "true")
                {
                    Log.Information("Attempting automatic migrations...");
                    using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
                    if (scope != null)
                    {
                        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                    }
                    else
                    {
                        Log.Error("Could log perform automatic migrations");
                    }
                }
            }

            app.UseHttpsRedirection();

            var contentTypes = new FileExtensionContentTypeProvider();
            contentTypes.Mappings.Add(".geojson", "application/geo+json");

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypes
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=API}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
