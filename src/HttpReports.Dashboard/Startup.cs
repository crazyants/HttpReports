using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpReports.Dashboard.DataAccessors;
using HttpReports.Dashboard.DataContext;
using HttpReports.Dashboard.Filters;
using HttpReports.Dashboard.Implements;
using HttpReports.Dashboard.Job;
using HttpReports.Dashboard.Models;
using HttpReports.Dashboard.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HttpReports.Dashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {  
             Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            { 
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });   

            DependencyInjection(services); 

            services.AddMvc(x => { 
                // ȫ�ֹ�����
                x.Filters.Add<GlobalAuthorizeFilter>();
                x.Filters.Add<GlobalExceptionFilter>();

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2); 

        }

         
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseStaticFiles();
            app.UseCookiePolicy(); 
            

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void DependencyInjection(IServiceCollection services)
        {
            services.AddSingleton<HttpReportsConfig>();
            services.AddSingleton<JobService>();
            services.AddSingleton<ScheduleService>();

            services.AddTransient<DBFactory>();

            services.AddTransient<DataService>(); 
          
            // ע�����ݿ������
            RegisterDBService(services);


            // ��ʼ��ϵͳ����
            InitWebService(services); 
        }

        private void InitWebService(IServiceCollection services)
        { 
            var provider = services.BuildServiceProvider();

            ServiceContainer.provider = provider; 

            // ��ʼ�����ݿ��
            provider.GetService<DBFactory>().InitDB();

            // ������̨����
            provider.GetService<JobService>().Start();

        } 

        private void RegisterDBService(IServiceCollection services)
        {
            string dbType = Configuration["HttpReportsConfig:DBType"]; 

            if (dbType.ToLower() == "sqlserver")
            {
                services.AddTransient<IDataAccessor, DataAccessorSqlServer>();
            }  
            else if (dbType.ToLower() == "mysql")
            {
                services.AddTransient<IDataAccessor,DataAccessorMySql>(); 
            }
            else
            {
                throw new Exception("���ݿ����ô���"); 
            }  
        }   
    }
}