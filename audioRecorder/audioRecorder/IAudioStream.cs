using System;
using Xamarin.Forms.Internals;

namespace audioRecorder
{
    public interface IAudioStream
    {
        /// <summary>
        /// Occurs when new audio has been streamed.
        /// </summary>
        event EventHandler<EventArg<byte[]>> OnBroadcast;
        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>
        /// The sample rate in hertz.
        /// </value>
        int SampleRate { get; }

        /// <summary>
        /// Gets the channel count.
        /// </summary>
        /// <value>
        /// The channel count.
        /// </value>
        int ChannelCount { get; }

        /// <summary>
        /// Gets bits per sample.
        /// </summary>
        int BitsPerSample { get; }

        #region Code for IMonitor work
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

        /// <summary>
        /// Plays a byte array passed into it in PCM format
        /// </summary>
        /// <param name="byteArray"></param>
        void PlayByteArrayAsync(byte[] byteArray);
        #endregion
    }
}

