using AuctionManagementService.Data;
using AuctionManagementService.IRepository;
using AuctionManagementService.Repository;
using AuctionManagementService.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AuctionManagementDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IKoiFishRepository, KoiFishRepository>();
builder.Services.AddScoped<IAuctionMethodRepository, AuctionMethodRepository>();
builder.Services.AddScoped<ILotStatusRepository, LotStatusRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IAuctionLotRepository, AuctionLotRepository>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://example.com") // Thay th? b?ng URL frontend c?a b?n
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cho ph�p cookie, header ???c g?i k�m
    });
});
var app = builder.Build();

app.UseMiddleware<AuthorizationMiddleware>();
app.UseCors("AllowSpecificOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();


app.Run();
