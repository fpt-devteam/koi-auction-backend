using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AuctionService.Helper
{
    public class HangfireHelper
    {
        public static void ClearHangfireData(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
            DELETE FROM [HangFire].[Job];
            DELETE FROM [HangFire].[State];
            DELETE FROM [HangFire].[JobParameter];
            DELETE FROM [HangFire].[JobQueue];
            DELETE FROM [HangFire].[Set];
            DELETE FROM [HangFire].[Hash];
            DELETE FROM [HangFire].[List];
            DELETE FROM [HangFire].[Counter];
            DELETE FROM [HangFire].[AggregatedCounter];
            DELETE FROM [HangFire].[Server];
            DELETE FROM [HangFire].[Schema];", connection);

                command.ExecuteNonQuery();
            }
        }
    }
}