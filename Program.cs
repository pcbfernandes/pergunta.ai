using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using LLM.Document.Response.Services;
using System.Configuration;
using LLM.Document.Response.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    })
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 30 * 1024 * 1024; // 30MB
    });

builder.Services.AddTelerikBlazor();
builder.Services.AddSingleton<LangChainService>();

var config = new AppConfig();
builder.Services.AddSingleton<AppConfig>(config);

var app = builder.Build();
app.Configuration.Bind("AppConfig", config);




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
