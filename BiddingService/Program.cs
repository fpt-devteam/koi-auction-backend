using BiddingService.Data;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.HandleMethod;
using BiddingService.Hubs;
using BiddingService.IRepositories;
using BiddingService.IServices;
using BiddingService.Repositories;
using BiddingService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Thêm HttpClient để WalletService có thể gửi yêu cầu đến API Gateway
builder.Services.AddHttpClient<WalletService>();
// Đăng ký các cấu hình từ appsettings.json
builder.Services.Configure<BiddingServiceOptionDtos>(
    builder.Configuration.GetSection("BiddingService"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

builder.Services.AddScoped<IBidStrategy, FixedPriceBidStrategy>();
builder.Services.AddScoped<IBidStrategy, SealedBidStrategy>();
builder.Services.AddScoped<IBidStrategy, AscendingBidStrategy>();
builder.Services.AddScoped<IBidStrategy, DescendingBidStrategy>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBidLogRepository, BidLogRepository>();
builder.Services.AddScoped<ISoldLotRepository, SoldLotRepository>();

builder.Services.AddScoped<ISoldLotService, SoldLotService>();
builder.Services.AddScoped<BidLogService>();
builder.Services.AddScoped<BidService>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddSingleton<BidManagementService>();

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
app.MapHub<BidHub>("/hub");
app.UseHttpsRedirection();
app.MapControllers();
app.UseCors();
app.Run();

