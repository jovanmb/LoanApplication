using Microsoft.EntityFrameworkCore;
using LoanApplication.Repositories;
using LoanApplication.API.Configurations;
using LoanApplication.API.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace LoanApplication.API;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                builder => builder
                    .WithOrigins("https://localhost:7230")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        // Add services to the container.
        builder.Services.AddDbContext<DataContext>(options =>
        options.UseInMemoryDatabase("CustomerDatabase"));

        builder.Services.Configure<BlacklistConfig>(builder.Configuration.GetSection("Blacklists"));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddScoped<ICustomerRequestRepository, CustomerRequestRepository>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null)
                    {
                        var ex = errorFeature.Error;
                        var errorResponse = new { Message = "An unexpected error occurred. Please try again later. \r\n" + ex.Message };
                        var errorJson = JsonSerializer.Serialize(errorResponse);

                        await context.Response.WriteAsync(errorJson);
                    }
                });
            });
        }

        app.UseCors("AllowSpecificOrigins");

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
