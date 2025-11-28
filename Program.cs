using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System;

var builder = WebApplication.CreateBuilder(args);

// Enable Razor Pages (for GUI)
builder.Services.AddRazorPages();

// Enable Controllers (for API endpoints) with camelCase JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Register HttpClient for Ollama
builder.Services.AddHttpClient("ollama", c =>
{
    c.BaseAddress = new Uri("http://localhost:11434/");
    c.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Exception handling and security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Optional: redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files (CSS, JS, index.html)
app.UseStaticFiles();

app.UseRouting();

// Map API controllers
app.MapControllers();

// Map Razor Pages (optional)
app.MapRazorPages();

app.Run();