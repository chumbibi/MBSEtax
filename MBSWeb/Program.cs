
using MBSWeb.Data;
using MBSWeb.Managers;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using MBSWeb.Services.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                                                options.UseSqlServer(builder.Configuration.GetConnectionString("connectionstring")));

            

            // ? SINGLE Identity registration
            builder.Services.AddIdentity<MBSUsers, MBSAccessRoles>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            //Register the Dependency Injections

            builder.Services.AddScoped<IUserAuthentication, UserAuthenticationRepository>();
            builder.Services.AddScoped<IInvoiceTransactions, InvoiceTransactionsRepository>();
            builder.Services.AddScoped<ICustomers, CustomersRepository>();
            builder.Services.AddScoped<ICompanies, CompaniesRepository>();



            //Register Managers
            builder.Services.AddScoped<UserAuthenticationManager>();
             builder.Services.AddScoped<InvoiceTransactionManager>();
            builder.Services.AddScoped<CustomerManager>();
            builder.Services.AddScoped<CompanyManager>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
                app.UseSwagger();
                app.UseSwaggerUI();

            app.UseCors(x =>
            {
                x.AllowAnyOrigin();//    
                x.WithOrigins(

                     "http://72.55.189.248:3057", 
                     "https://localhost:5173",
                     "http://localhost:5173",
                     "https://72.55.189.248:3057",
                    "https://jhutes2.cyberspace.net.ng");  //These are what we need to use:
                x.AllowAnyMethod();
                x.AllowAnyHeader();
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
