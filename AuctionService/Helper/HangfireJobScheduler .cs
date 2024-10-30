using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;

namespace AuctionService.Helper
{
    public class HangfireJobScheduler : IHangfireJobScheduler
    {
        TimeSpan offset = new TimeSpan(7, 0, 0);
        public void Schedule(Expression<Func<Task>> methodCall, DateTime enqueueAt)
        {
            System.Console.WriteLine($"Scheduling job at {enqueueAt.ConvertToDateTimeOffset(offset)}");
            BackgroundJob.Schedule(methodCall, enqueueAt.ConvertToDateTimeOffset(offset));
        }

        public void Schedule(Expression<Action> methodCall, DateTime enqueueAt)
        {
            System.Console.WriteLine($"Scheduling job at {enqueueAt.ConvertToDateTimeOffset(offset)}");
            BackgroundJob.Schedule(methodCall, enqueueAt.ConvertToDateTimeOffset(offset));
        }
    }
}