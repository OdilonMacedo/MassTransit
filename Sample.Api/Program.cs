using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Components.Consumers;
using Sample.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
builder.Services.AddMassTransit(cfg =>
{
    //cfg.usingrabbitmq((ctx, busconfigurator) =>
    //{
    //    busconfigurator.host(builder.configuration.getconnectionstring("rabbitmq"));
    //});
    cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq());
    cfg.AddRequestClient<SubmitOrder>();
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
