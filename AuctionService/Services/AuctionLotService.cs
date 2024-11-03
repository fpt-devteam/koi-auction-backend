using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.ScheduledTask;
using AuctionService.Enums;
using AuctionService.HandleMethod;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;
// using Hangfire;

namespace AuctionService.Services
{
    public class AuctionLotService : IAuctionLotService
    {
        private const int BREAK_TIME = 1;
        private const int EXTENDED_TIME = 20;

        private IAuctionService _auctionService;

        private readonly BidManagementService _bidManagementService;

        private readonly ITaskSchedulerService _taskSchedulerService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AuctionLotService(IAuctionService auctionService, BidManagementService bidManagementService, ITaskSchedulerService taskSchedulerService, IServiceScopeFactory serviceScopeFactory)
        {
            _bidManagementService = bidManagementService;
            _taskSchedulerService = taskSchedulerService;
            _serviceScopeFactory = serviceScopeFactory;
            _auctionService = auctionService;
        }

        public async Task ScheduleAuctionLotAsync(int auctionLotId, DateTime startTime)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                AuctionLot auctionLot = await unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                auctionLot.StartTime = startTime;
                auctionLot.AuctionLotStatusId = (int)Enums.AuctionLotStatus.Scheduled;

                if (!await unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the data");
                }
            }
            // _taskSchedulerService.ScheduleTask(() => StartAuctionLotAsync(auctionLotId), startTime);
            _taskSchedulerService.ScheduleTask(new ScheduledTask
            {
                ExecuteAt = startTime,
                Task = async () => await StartAuctionLotAsync(auctionLotId)
            });
            System.Console.WriteLine($"Auction lot {auctionLotId} is scheduled to start at {startTime}");
        }
        public async Task StartAuctionLotAsync(int auctionLotId)
        {
            Console.WriteLine($"Auction lot {auctionLotId} is starting!");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                AuctionLot auctionLot = await unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                auctionLot.AuctionLotStatusId = (int)Enums.AuctionLotStatus.Ongoing;

                if (!await unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the data");
                }

                //set up auction bid dto in bid service
                AuctionLotBidDto auctionLotBidDto = auctionLot.ToAuctionLotBidDtoFromAuctionLot();
                await _bidManagementService.StartAuctionLotAsync(auctionLotBidDto);



                DateTime endTimePredict = auctionLot.StartTime!.Value.Add(auctionLot.Duration);
                // Schedule extended time auction lot
                switch (auctionLotBidDto.AuctionMethodId)
                {
                    case (int)BidMethodType.FixedPrice:
                        _taskSchedulerService.ScheduleTask(new ScheduledTask
                        {
                            ExecuteAt = endTimePredict,
                            Task = async () => await EndAuctionLotAsync(auctionLotId)
                        });
                        break;
                    case (int)BidMethodType.SealedBid:
                        _taskSchedulerService.ScheduleTask(new ScheduledTask
                        {
                            ExecuteAt = endTimePredict,
                            Task = async () => await EndAuctionLotAsync(auctionLotId)
                        });
                        break;
                    case (int)BidMethodType.AscendingBid:
                        if (_bidManagementService.BidService!.CurrentStrategy is AscendingBidStrategy strategyAsc)
                        {
                            strategyAsc.CountdownFinished += async (id) => await EndAuctionLotAsync(auctionLotId);
                        }
                        break;
                    case (int)BidMethodType.DescendingBid:
                        if (_bidManagementService.BidService!.CurrentStrategy is DescendingBidStrategy strategyDesc)
                        {
                            strategyDesc.CountdownFinished += async (id) => await EndAuctionLotAsync(auctionLotId);
                        }
                        break;
                    // Thêm các case khác ở đây nếu có
                    default:
                        throw new ArgumentException("Invalid auctionLotMethodId");
                }
                // DateTime startExtendedTime = auctionLot.StartTime!.Value.Add(auctionLot.Duration).AddSeconds(-3 * EXTENDED_TIME);
                // ScheduleExentedPhase(auctionLotId, startExtendedTime);
            }
        }
        // public void ScheduleExentedPhase(int auctionLotId, DateTime startExtendedTime)
        // {
        //     // _jobScheduler.Schedule(() => StartExtendedPhase(auctionLotId), startExtendedTime);
        //     _taskSchedulerService.ScheduleTask(new ScheduledTask
        //     {
        //         ExecuteAt = startExtendedTime,
        //         Action = () => StartExtendedPhase(auctionLotId)
        //     });
        //     System.Console.WriteLine($"Auction lot {auctionLotId} is scheduled to start extended phase at {startExtendedTime}");
        // }
        // public void StartExtendedPhase(int auctionLotId)
        // {
        //     _bidManagementService.BidService!.CountdownFinished += (id) => EndAuctionLotAsync(auctionLotId);
        //     _bidManagementService.BidService!.StartExtendPhase();
        // }



        public async Task EndAuctionLotAsync(int auctionLotId)
        {
            Console.WriteLine($"Auction lot {auctionLotId} is ending!");
            AuctionLot? auctionLot = null;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                //unit of work
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                auctionLot = await unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
                auctionLot.AuctionLotStatusId = (int)Enums.AuctionLotStatus.Ended;
                auctionLot.EndTime = DateTime.Now;

                if (!await unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the data");
                }
            }

            await _bidManagementService.EndAuctionLotAsync();

            AuctionLot? nextLot = null;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                nextLot = await unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(auctionLot.AuctionId, auctionLot.OrderInAuction + 1);
                if (nextLot != null)
                {
                    DateTime? nextStartTime = auctionLot.EndTime?.AddMinutes(BREAK_TIME);
                    await ScheduleAuctionLotAsync(nextLot.AuctionLotId, nextStartTime!.Value);
                }
                else
                {
                    // Nếu là AuctionLot cuối cùng trong phiên đấu giá thì cập nhật trạng thái phiên đấu giá
                    await _auctionService.EndAuctionAsync(auctionLot.AuctionId);
                }
                if (!await unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the data");
                }
            }

        }

    }
}

/*
ScheduleAuctionLot 
    lên lịch để StartActionLot
    cập nhật startTime của AuctionLot trong cơ sở dữ liệu

StartAuctionLot sẽ cập nhật 
    status, 
    set up auction bid dto in bid service
    thông báo qua SignalR -> auction lot is starting

ScheduleExentedPhase 
    lên lịch để StartExtendedPhase

StartExtendedPhase
    set up event handler cho CountdownFinished
    StartExtendPhase trong BidService

EndAuctionLot sẽ cập nhật
    status,
    endTime của AuctionLot trong cơ sở dữ liệu
    ScheduleAuctionLot cho AuctionLot tiếp theo
    hoặc EndAuction nếu là AuctionLot cuối cùng
    thông báo qua SignalR -> auction lot is ending
*/

