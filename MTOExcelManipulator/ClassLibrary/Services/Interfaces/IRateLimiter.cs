using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Interfaces
{
    public interface IRateLimiter
    {
        /// <summary>
        /// Executes the provided asynchronous action while enforcing rate limiting.
        /// </summary>
        /// <param name="action">The asynchronous action to perform.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PerformAsync(Func<Task> action);

        /// <summary>
        /// Configures the maximum number of allowed calls and the time window for the rate limiter.
        /// </summary>
        /// <param name="maxCalls">The maximum number of calls allowed in the specified time window.</param>
        /// <param name="timeWindow">The time window within which the maximum calls are counted.</param>
        void Configure(int maxCalls, TimeSpan timeWindow);
    }
}
