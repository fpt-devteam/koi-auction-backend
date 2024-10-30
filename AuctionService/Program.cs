
using AuctionService.Controller;
using AuctionService.IRepository;
using AuctionService.Repository;
using AuctionService.Middlewares;
using Microsoft.EntityFrameworkCore;
using AuctionService.Data;
using AuctionService.IServices;
using AuctionService.Services;
// using Hangfire;
using AuctionService.HandleMethod;
using AuctionService.Dto.UserConnection;
using AuctionService.Hubs;
using AuctionService.Helper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<WalletService>();
// Đăng ký các cấu hình từ appsettings.json
builder.Services.Configure<BiddingServiceOptionDtos>(
    builder.Configuration.GetSection("BiddingService"));
builder.Services.AddSingleton<IDictionary<string, UserConnectionDto>>(_ => new Dictionary<string, UserConnectionDto>());
builder.Services.AddSignalR();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
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
builder.Services.AddScoped<IBidStrategy, FixedPriceBidStrategy>();
builder.Services.AddScoped<IBidStrategy, SealedBidStrategy>();
builder.Services.AddScoped<IBidStrategy, AscendingBidStrategy>();
builder.Services.AddScoped<IBidStrategy, DescendingBidStrategy>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IKoiFishRepository, KoiFishRepository>();
builder.Services.AddScoped<IAuctionMethodRepository, AuctionMethodRepository>();
builder.Services.AddScoped<ILotStatusRepository, LotStatusRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IAuctionLotRepository, AuctionLotRepository>();
builder.Services.AddScoped<IBidLogRepository, BidLogRepository>();
builder.Services.AddScoped<ISoldLotRepository, SoldLotRepository>();


builder.Services.AddScoped<ISoldLotService, SoldLotService>();
builder.Services.AddScoped<IBidLogService, BidLogService>();
builder.Services.AddScoped<BidService>();
builder.Services.AddScoped<WalletService>();

builder.Services.AddSingleton<BidManagementService>();
builder.Services.AddScoped<IAuctionService, AuctionService.Services.AuctionService>();
builder.Services.AddScoped<IAuctionLotService, AuctionLotService>();

builder.Services.AddSingleton<ITaskSchedulerService, TaskSchedulerService>();


builder.Services.AddScoped<BreederDetailService>();

builder.Services.AddScoped<IAuctionStatusRepository, AuctionStatusRepository>();
builder.Services.AddScoped<IAuctionLotStatusRepository, AuctionLotStatusRepository>();
// Đăng ký MemoryCache
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5501", "http://localhost:3002") // Thay th? b?ng URL frontend c?a b?n
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

app.MapHub<BidHub>("/hub");
app.UseHttpsRedirection();
app.MapControllers();

// app.UseHangfireDashboard("/hangfire");

app.Run();
