using Hfs.Serve.Core.Handler;
using Hfs.Server.Core.Common;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();


// Add services to the container.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressInferBindingSourcesForParameters = true;
});

var app = builder.Build();

//Imposta esecuzione principale
app.Map("/hfs/FileServer.ashx", async (HttpContext context) =>
{
    await context.HandleHfsRequest();
});
app.Map("/hfs", async (HttpContext context) =>
{
    await context.HandleHfsRequest();
});

//Imposta routine di shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    HfsData.Stop();
});

//Appoggia dati utili
HfsData.WebApp = app;

//Avvia ambiente
HfsData.Init();

//Infine avvia
app.Run();

