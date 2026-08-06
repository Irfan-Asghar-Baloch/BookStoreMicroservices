using Microsoft.EntityFrameworkCore;
using OrderService.Entities;
using OrderService.Interface;
using OrderService.Interface.IOrderRepository;
using OrderService.Interface.IOrderService;
using OrderService.Messaging;
using OrderService.RepositoryImplementation;
using OrderService.ServiceImplementation;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<IBookApiService, BookApiService>(client => { client.BaseAddress = new Uri("https://localhost:7289/"); });
StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbConnectionString"));
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrdersService>();
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
