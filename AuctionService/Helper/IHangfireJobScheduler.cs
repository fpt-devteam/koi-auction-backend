using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AuctionService.Helper
{
    public interface IHangfireJobScheduler
    {
        void Schedule(Expression<Func<Task>> methodCall, DateTime enqueueAt);

        void Schedule(Expression<Action> methodCall, DateTime enqueueAt);
    }
}