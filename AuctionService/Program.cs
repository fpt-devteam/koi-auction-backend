
using AuctionService.Controller;
using AuctionService.IRepository;
using AuctionService.Repository;
using AuctionService.Middlewares;
using Microsoft.EntityFrameworkCore;
using AuctionService.Data;
using AuctionService.IServices;
using AuctionService.Services;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddTransient<BreederDetailController>();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
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
builder.Services.AddScoped<IAuctionService, AuctionService.Services.AuctionService>();
builder.Services.AddScoped<IAuctionLotService, AuctionLotService>();

// hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnectionStringDB"));
});
builder.Services.AddHangfireServer();


builder.Services.AddScoped<IAuctionStatusRepository, AuctionStatusRepository>();
builder.Services.AddScoped<IAuctionLotStatusRepository, AuctionLotStatusRepository>();
// Đăng ký MemoryCache
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://example.com") // Thay th? b?ng URL frontend c?a b?n
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

    });
});

builder.Services.AddHttpContextAccessor();
// Thêm HttpClient vào DI
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseMiddleware<AuthorizationMiddleware>();
app.UseCors("AllowSpecificOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHangfireDashboard("/hangfire");

app.Run();
