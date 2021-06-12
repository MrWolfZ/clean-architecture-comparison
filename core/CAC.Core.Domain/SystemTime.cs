using System;
using System.Threading;

namespace CAC.Core.Domain
{
    public static class SystemTime
    {
        private static readonly AsyncLocal<DateTimeOffset?> CurrentTimeAsyncLocal = new();

        public static DateTimeOffset Now => CurrentTimeAsyncLocal.Value ?? DateTimeOffset.UtcNow;

        public static IDisposable WithCurrentTime(DateTimeOffset time)
        {
            CurrentTimeAsyncLocal.Value = time;

            return new Disposable(() => CurrentTimeAsyncLocal.Value = null);
        }

        private sealed class Disposable : IDisposable
        {
            private readonly Action disposeFn;

            public Disposable(Action disposeFn)
            {
                this.disposeFn = disposeFn;
            }

            public void Dispose() => disposeFn();
        }
    }
}
