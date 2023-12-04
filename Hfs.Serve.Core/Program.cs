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
app.Map("/hfs", async (HttpContext context) =>
{
    await HfsHandler.Handle(context);
});

//Imposta routine di shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    HfsData.Stop();
});

//Appoggia dati utili
HfsData.HostingEnv = app.Environment;
HfsData.WebApp = app;
HfsData.WebLogger = app.Logger;

//Avvia ambiente
HfsData.Init();

//Infine avvia
app.Run();

