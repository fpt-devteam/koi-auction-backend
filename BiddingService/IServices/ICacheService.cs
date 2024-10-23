namespace BiddingService.IServices
{
    public interface ICacheService
    {
        // Lấy balance của user từ cache
        int GetBalance(int userId);

        // Lưu balance của user vào cache với thời gian hết hạn
        void SetBalance(int userId, int balance, TimeSpan expiration);

        // Xóa balance của user khỏi cache
        void RemoveBalance(int userId);
        // Check userId có Balance trong cache chưa
        bool IsUserBalanceInCache(int userId);
    }
}