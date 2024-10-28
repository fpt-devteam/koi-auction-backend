
namespace AuctionService.Middlewares
{
    public class AuthorizationMiddleware
    {
        public enum UserRole
        {
            Member = 1,
            Breeder = 2,
            Staff = 3,
            Admin = 4
        }
        private readonly RequestDelegate _next;
        // được đăng kí trong khi app.UseMiddleware<MyCustomMiddleware>() trong Program.cs

        private readonly Dictionary<string, Dictionary<string, UserRole[]>> _routePermissions = new()
        {

            { "/api/bid-log", new Dictionary<string, UserRole[]>
                {
                    { HttpMethods.Get, new[] { UserRole.Staff,  UserRole.Admin } }
                }
            },
            { "/api/bid-log/{id}", new Dictionary<string, UserRole[]>
                {
                    { HttpMethods.Get, new[] { UserRole.Staff,  UserRole.Admin } }
                }
            }
        };
        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy thông tin userid từ header (API Gateway truyền vào)
            var userId = context.Request.Headers["uid"].FirstOrDefault();
            var userRoleId = context.Request.Headers["uri"].FirstOrDefault();
            // Lấy thông tin về phương thức HTTP và route đang được yêu cầu
            var httpMethod = context.Request.Method;
            var route = context.Request.Path.Value;

            Console.WriteLine($"User ID: {userId}, Role: {userRoleId}, Route: {route}");

            // Kiểm tra nếu route không cần phân quyền (không có trong Dictionary)
            if (!_routePermissions.ContainsKey(route!) || !_routePermissions[route!].ContainsKey(httpMethod))
            {
                await _next(context);
                return;
            }

            // Nếu userRoleId là null hoặc empty, chặn truy cập cho các route cần phân quyền
            if (string.IsNullOrEmpty(userRoleId) || !int.TryParse(userRoleId, out int roleId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access denied. Missing or invalid role ID.");
                return;
            }

            // Chuyển userRoleId thành Enum UserRole
            if (!Enum.IsDefined(typeof(UserRole), roleId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid role ID.");
                return;
            }

            var userRole = (UserRole)roleId; // Chuyển đổi int thành Enum

            // Kiểm tra quyền hạn dựa trên route và phương thức HTTP
            if (_routePermissions.ContainsKey(route!) && _routePermissions[route!].ContainsKey(httpMethod))
            {
                var allowedRoles = _routePermissions[route!][httpMethod];
                if (!allowedRoles.Contains(userRole))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Only {string.Join(", ", allowedRoles)} can access this route.");
                    return;
                }
            }
            // Nếu người dùng có quyền, chuyển tiếp đến middleware tiếp theo
            await _next(context);
        }

    }
}