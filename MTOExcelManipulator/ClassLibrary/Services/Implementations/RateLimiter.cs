using ClassLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Implementations
{
    public class RateLimiter : IRateLimiter
    {
        private int _maxCalls;
        private TimeSpan _timeWindow;
        private int _callCount;
        private DateTime _windowStart;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public RateLimiter(int maxCalls, TimeSpan timeWindow)
        {
            if (maxCalls <= 0)
                throw new ArgumentException("Max calls must be greater than 0.", nameof(maxCalls));
            if (timeWindow <= TimeSpan.Zero)
                throw new ArgumentException("Time window must be greater than 0.", nameof(timeWindow));

            _maxCalls = maxCalls;
            _timeWindow = timeWindow;
            _callCount = 0;
            _windowStart = DateTime.UtcNow;
        }

        public void Configure(int maxCalls, TimeSpan timeWindow)
        {
            if (maxCalls <= 0)
                throw new ArgumentException("Max calls must be greater than 0.", nameof(maxCalls));
            if (timeWindow <= TimeSpan.Zero)
                throw new ArgumentException("Time window must be greater than 0.", nameof(timeWindow));

            _semaphore.Wait();
            try
            {
                _maxCalls = maxCalls;
                _timeWindow = timeWindow;

                // Reset state to ensure consistent behavior after reconfiguration.
                _callCount = 0;
                _windowStart = DateTime.UtcNow;

                Console.WriteLine($"Rate limiter reconfigured: max calls = {_maxCalls}, time window = {_timeWindow.TotalSeconds} seconds.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task PerformAsync(Func<Task> action)
        {
            await EnsureRateLimitAsync();

            try
            {
                await action();
            }
            finally
            {
                _callCount++;
            }
        }

        private async Task EnsureRateLimitAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var now = DateTime.UtcNow;

                if (now - _windowStart > _timeWindow)
                {
                    _windowStart = now;
                    _callCount = 0;
                }

                if (_callCount >= _maxCalls)
                {
                    var waitTime = _windowStart.Add(_timeWindow) - now;
                    Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalSeconds} seconds...");
                    await Task.Delay(waitTime);
                    _windowStart = DateTime.UtcNow;
                    _callCount = 0;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}