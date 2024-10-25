using BiddingService.Data;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.Hubs;
using BiddingService.IRepositories;
using BiddingService.IServices;
using BiddingService.Repositories;
using BiddingService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDictionary<string, UserConnectionDto>>(_ => new Dictionary<string, UserConnectionDto>());
// Cấu hình CORS cho phép bất kỳ domain nào

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()  // Cho phép cookie và xác thực
               .SetIsOriginAllowed(origin => true);  // Cho phép mọi nguồn (hoặc tùy chỉnh nguồn cần thiết)
    });
});
builder.Services.AddSignalR();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddDbContext<BiddingDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBidLogRepository, BidLogRepository>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IBidLogService, BidLogService>();
builder.Services.AddScoped<AuctionLotBidService>();
builder.Services.AddSingleton<AuctionLotService>();

//add swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BiddingService", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<AuctionLotHub>("/hub");
app.UseHttpsRedirection();
app.MapControllers();
app.UseCors();
app.Run();

