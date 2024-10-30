using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuctionService.Dto.ScheduledTask;
using AuctionService.IServices;

namespace AuctionService.Services
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        private readonly ILogger<TaskSchedulerService> _logger;
        private readonly Dictionary<Guid, CancellationTokenSource> _scheduledTasks;

        public TaskSchedulerService(ILogger<TaskSchedulerService> logger)
        {
            _logger = logger;
            _scheduledTasks = new Dictionary<Guid, CancellationTokenSource>();
        }

        public Guid ScheduleTask(ScheduledTask task)
        {
            var cts = new CancellationTokenSource();
            Guid taskId = Guid.NewGuid();
            // DateTime currentUtcTime = DateTime.UtcNow;
            // _logger.LogInformation($"Current UTC Time: {currentUtcTime:o}");
            // _logger.LogInformation($"Scheduled ExecuteAt: {task.ExecuteAt:o}");


            TimeSpan timeToExecute = task.ExecuteAt - DateTime.Now;
            // _logger.LogInformation($"Calculated delay (timeToExecute): {timeToExecute.TotalSeconds} seconds");


            if (timeToExecute <= TimeSpan.Zero)
            {
                _logger.LogWarning($"Task {taskId} is scheduled in the past. Executing immediately.");
                timeToExecute = TimeSpan.Zero;
            }
            // System.Console.WriteLine("35");
            _ = ExecuteScheduledTask(taskId, task, timeToExecute, cts.Token);
            _scheduledTasks[taskId] = cts;

            _logger.LogInformation($"Task {taskId} scheduled to run at {task.ExecuteAt} (in {timeToExecute.TotalMinutes} minutes).");
            return taskId;
        }

        private async Task ExecuteScheduledTask(Guid taskId, ScheduledTask scheduledTask, TimeSpan timeToExecute, CancellationToken cancellationToken)
        {
            try
            {
                // System.Console.WriteLine("47");
                System.Console.WriteLine();
                await Task.Delay(timeToExecute, cancellationToken); // Đợi cho đến khi đến thời điểm thực thi hoặc bị hủy
                // System.Console.WriteLine("49");
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Thực hiện hàm bất đồng bộ nếu có, nếu không thì thực hiện hàm đồng bộ
                    if (scheduledTask.Task != null)
                    {
                        try
                        {
                            await scheduledTask.Task.Invoke(); // Đảm bảo gọi hàm async và await đầy đủ
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"An error occurred while executing Task {taskId}");
                        }
                    }
                    else if (scheduledTask.Action != null)
                    {
                        try
                        {
                            scheduledTask.Action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"An error occurred while executing Task {taskId}");
                        }
                    }

                    _logger.LogInformation($"Task {taskId} completed successfully.");
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"Task {taskId} was canceled before execution.");
            }
            finally
            {
                // Xóa tác vụ khỏi danh sách sau khi hoàn thành hoặc bị hủy
                _scheduledTasks.Remove(taskId);
                _logger.LogInformation($"Task {taskId} removed from scheduled tasks.");
            }
        }

        public void CancelScheduledTask(Guid taskId)
        {
            if (_scheduledTasks.TryGetValue(taskId, out var cts))
            {
                cts.Cancel(); // Hủy tác vụ
                _scheduledTasks.Remove(taskId); // Loại bỏ khỏi danh sách tác vụ đã lên lịch
                _logger.LogInformation($"Task {taskId} has been canceled.");
            }
            else
            {
                _logger.LogWarning($"Task {taskId} not found.");
            }
        }


    }
}