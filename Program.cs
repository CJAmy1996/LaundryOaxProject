using LaundryOaxWebAPI.Controllers;
using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.Services.OrderServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
 


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LaundryOaxDBContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("LaundryOaxConnectionString")));
builder.Services.AddScoped<IOrderService, OrderService>();
var CorsPolicy = "CorsPolicy";

builder.Services.AddCors(opt =>
{

    opt.AddPolicy(name: CorsPolicy, builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();

app.MapControllers();



app.Run();
