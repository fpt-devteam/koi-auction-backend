namespace AuctionManagementService.IRepository
{
    public interface IUnitOfWork : IDisposable
{
    IAuctionLotRepository AuctionLots { get; }
    IAuctionMethodRepository AuctionMethods { get; }
    IAuctionRepository Auctions { get; }
    IKoiFishRepository KoiFishes { get; }
    IKoiMediaRepository KoiMedia { get; }
    ILotRepository Lots { get; }
    ILotStatusRepository LotStatuses { get; }

    void SaveChanges();
}

}