using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Extensions
{
    public static class TaskWorkerExtensions
    {
        public static void Start(this TaskWorker taskWorker, CancellationToken linkedToken = default)
        {
            var tokens = new CancellationToken[1] { linkedToken };
            
            try
            {
                taskWorker.Start(tokens);
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static void Start(this TaskWorker taskWorker, IEnumerable<CancellationToken> linkedTokens)
        {
            try
            {
                taskWorker.Start(linkedTokens.ToArray());
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static async Task StartAsync(
            this TaskWorker     taskWorker, 
            CancellationToken   linkedToken         = default, 
            CancellationToken   cancellationToken   = default
        )
        {
            var tokens = new CancellationToken[1] { linkedToken };

            try
            {
                await taskWorker.StartAsync(tokens, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static async Task StartAsync(
            this TaskWorker                 taskWorker,
            IEnumerable<CancellationToken>  linkedTokens,
            CancellationToken               cancellationToken = default
        )
        {
            try
            {
                await taskWorker.StartAsync(linkedTokens.ToArray(), cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }
    }
}
