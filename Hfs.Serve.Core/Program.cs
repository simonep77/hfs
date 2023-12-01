using Hfs.Serve.Core.src.Handler;
using Hfs.Server.Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressInferBindingSourcesForParameters = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}


app.Map("/hfs", async (HttpContext context) =>
{
    await HfsHandler.Handle(context);
    //return Results.Ok();
});

//Appoggia dati utili
HfsData.HostingEnv = app.Environment;
HfsData.WebApp = app;

HfsData.Init();

//Infine avvia
app.Run();

