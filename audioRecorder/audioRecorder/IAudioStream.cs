using System;
using Xamarin.Forms.Internals;

namespace audioRecorder
{
    public interface IAudioStream : IMonitor
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

        /// <summary>
        /// Gets the average data transfer rate
        /// </summary>
        /// <value>The average data transfer rate in bytes per second.</value>
        int AverageBytesPerSecond { get; }
    }
}

