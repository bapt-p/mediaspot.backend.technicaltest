using Mediaspot.Infrastructure;
using Mediaspot.Application;
using Mediaspot.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure("Mediaspot.Backend.TechnicalTest");

builder.Services.AddHostedService<TranscodeWorker>();

IHost host = builder.Build();

host.Run();