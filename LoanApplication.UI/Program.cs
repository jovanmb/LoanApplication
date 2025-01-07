using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoanApplication.UI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient();
        builder.Services.AddSession(options => 
        { 
            options.IdleTimeout = TimeSpan.FromMinutes(60); 
            options.Cookie.HttpOnly = true; 
            options.Cookie.IsEssential = true; 
        });

        // Add services to the container.
        builder.Services.AddRazorPages()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        var app = builder.Build();

        app.UseSession();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
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
                        var errorResponse = new { Message = "An unexpected error occurred. Please try again later." };
                        var errorJson = JsonSerializer.Serialize(errorResponse);

                        // Log the error if needed (optional)
                        // _logger.LogError(ex, "Unhandled exception");

                        await context.Response.WriteAsync(errorJson);
                    }
                });
            });

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
