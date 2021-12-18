using System;
using Xamarin.Forms.Internals;

namespace audioRecorder
{
    public interface IMonitor
    {
        /// <summary>
        /// Event handler for active status changes
        /// </summary>
        event EventHandler<EventArg<bool>> OnActiveChanged;

        /// <summary>
        /// Event handler for exceptions
        /// </summary>
        event EventHandler<EventArg<Exception>> OnException;

        /// <summary>
        /// Gets a value indicating whether the monitor is active
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Start monitoring.
        /// </summary>
        /// <returns>True when monitor starts, otherwise false</returns>
        bool Start();

        /// <summary>
        /// Stop monitoring.
        /// </summary>
        void Stop();

        void PlayByteArrayAsync(byte[] byteArray);
    }
}
