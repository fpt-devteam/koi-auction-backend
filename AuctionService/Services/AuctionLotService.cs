using System;
using System.Net.Http;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionLot;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using Hangfire;

namespace AuctionService.Services
{
    public class AuctionLotService : IAuctionLotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;

        private const int AUCTION_LOT_STATUS_ONGOING = 2;
        private const int AUCTION_LOT_STATUS_ENDED = 3;
        private const int AUCTION_STATUS_ENDED = 3;
        private const int BREAK_TIME = 1;
        private const string BIDDING_SERVICE_URL = "http://localhost:3000/bidding-service";

        public AuctionLotService(IUnitOfWork unitOfWork, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
        }

        public async Task ScheduleAuctionLot(int auctionLotId, DateTime startTime)
        {
            try
            {
                Console.WriteLine($"Scheduling AuctionLot {auctionLotId} at {startTime}");
                var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                if (auctionLot == null)
                {
                    throw new Exception($"Auction Lot with ID {auctionLotId} not found.");
                }

                TimeSpan timeToStart = startTime - DateTime.Now;
                if (timeToStart <= TimeSpan.Zero)
                {
                    timeToStart = new TimeSpan(0, 0, 5);
                }
                auctionLot = _unitOfWork.AuctionLots.UpdateStartTime(auctionLot, startTime);
                DateTime endTime = startTime.Add(auctionLot.Duration.ToTimeSpan());
                auctionLot = _unitOfWork.AuctionLots.UpdateEndTime(auctionLot, endTime);

                BackgroundJob.Schedule(() => StartAuctionLotWrapper(auctionLotId), timeToStart);

                // await _unitOfWork.AuctionLotJobs.CreateAsync(new Models.AuctionLotJob { AuctionLotId = auctionLotId, HangfireJobId = jobId });
                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the data.");
                }
                System.Console.WriteLine($"Time to start: {timeToStart}");
                Console.WriteLine($"AuctionLot {auctionLotId} scheduled from {startTime} to {endTime}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to schedule AuctionLot {auctionLotId}: {ex.Message}");
                throw;
            }
        }

        public void StartAuctionLotWrapper(int auctionLotId)
        {
            // Wrap the async call in Task.Run and wait for it to complete
            Task.Run(() => StartAuctionLot(auctionLotId)).Wait();
        }
        public async Task StartAuctionLot(int auctionLotId)
        {
            try
            {
                Console.WriteLine($"Starting Auction Lot {auctionLotId}");
                var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                if (auctionLot == null)
                {
                    throw new Exception($"Auction Lot {auctionLotId} not found.");
                }

                auctionLot = _unitOfWork.AuctionLots.UpdateStatus(auctionLot, AUCTION_LOT_STATUS_ONGOING);
                // if (!await _unitOfWork.SaveChangesAsync())
                // {
                //     throw new Exception("Failed to update Auction Lot status.");
                // }
                AuctionLotBidDto auctionLotBidDto = auctionLot.ToAuctionLotBidDtoFromAuctionLot();
                var response = await _httpClient.PostAsJsonAsync($"{BIDDING_SERVICE_URL}/bid/start-auction-lot", auctionLotBidDto);

                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("Failed to update Auction Lot status.");
                }
                await ScheduleEndAuctionLot(auctionLotId, auctionLot.EndTime!.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start Auction Lot {auctionLotId}: {ex.Message}");
                throw;
            }
        }

        public async Task ScheduleEndAuctionLot(int auctionLotId, DateTime endTime)
        {
            try
            {
                TimeSpan timeToEnd = endTime - DateTime.Now;
                if (timeToEnd <= TimeSpan.Zero)
                {
                    timeToEnd = new TimeSpan(0, 0, 5);
                }
                var jobId = BackgroundJob.Schedule(() => EndAuctionLotWrapper(auctionLotId), timeToEnd);

                await _unitOfWork.AuctionLotJobs.UpdateAsync(auctionLotId, jobId);
                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while updating the AuctionLotJob.");
                }
                System.Console.WriteLine($"Scheduled end of AuctionLot {auctionLotId} at {endTime}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to schedule end of AuctionLot {auctionLotId}: {ex.Message}");
                throw;
            }
        }

        public void EndAuctionLotWrapper(int auctionLotId)
        {
            // Wrap the async call in Task.Run and wait for it to complete
            Task.Run(() => EndAuctionLot(auctionLotId)).Wait();
        }
        public async Task EndAuctionLot(int auctionLotId)
        {
            try
            {
                Console.WriteLine($"Ending Auction Lot {auctionLotId}");
                var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                if (auctionLot == null)
                {
                    throw new Exception($"Auction Lot {auctionLotId} not found.");
                }

                auctionLot = _unitOfWork.AuctionLots.UpdateStatus(auctionLot, AUCTION_LOT_STATUS_ENDED);
                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("Failed to update Auction Lot status.");
                }
                await _httpClient.GetAsync($"{BIDDING_SERVICE_URL}/bid/end-auction-lot");


                int nextOrderInAuction = auctionLot.OrderInAuction + 1;
                var nextLotAuction = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(auctionLot.AuctionId, nextOrderInAuction);
                if (nextLotAuction != null)
                {
                    var nextStartTime = auctionLot.EndTime!.Value.AddMinutes(BREAK_TIME);
                    await ScheduleAuctionLot(nextLotAuction.AuctionLotId, nextStartTime);
                }
                else
                {
                    await EndAuction(auctionLot.AuctionId, auctionLot.EndTime!.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to end Auction Lot {auctionLotId}: {ex.Message}");
                throw;
            }
        }

        public async Task EndAuction(int auctionId, DateTime endTime)
        {
            try
            {
                Console.WriteLine($"Ending Auction {auctionId}");
                await _unitOfWork.Auctions.UpdateStatusAsync(auctionId, AUCTION_STATUS_ENDED);
                await _unitOfWork.Auctions.UpdateEndTimeAsync(auctionId, endTime);

                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the auction data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to end Auction {auctionId}: {ex.Message}");
                throw;
            }
        }

        public Task UpdateEndTimeAuctionLot(int auctionLotId, DateTime newEndTime)
        {
            throw new NotImplementedException();
        }
    }
}
