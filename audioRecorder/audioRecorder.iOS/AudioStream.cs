using System;
using audioRecorder.iOS;
using AudioToolbox;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
[assembly: Dependency(typeof(AudioStream))]
namespace audioRecorder.iOS
{
    public class AudioStream : IAudioStream
    {
        private InputAudioQueue audioQueue;

        private readonly int bufferSize;

        #region IAudioStream implementation

        public event EventHandler<EventArg<byte[]>> OnBroadcast;

        public int AverageBytesPerSecond
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SampleRate
        {
            get;
            private set;
        }

        public int ChannelCount
        {
            get
            {
                return 1;
            }
        }

        public int BitsPerSample
        {
            get
            {
                return 16;
            }
        }

        #endregion

        #region IMonitor implementation

        public event EventHandler<EventArg<bool>> OnActiveChanged;

        public event EventHandler<EventArg<Exception>> OnException;

        public bool Start()
        {
            return (this.audioQueue.Start() == AudioQueueStatus.Ok);
        }

        public void Stop()
        {
            this.audioQueue.Stop(true);
        }

        public bool Active
        {
            get
            {
                return this.audioQueue.IsRunning;
            }
        }

        #endregion

        public AudioStream()
        {
            this.SampleRate = 8000;
            this.bufferSize = 2048;
            this.Init();

        }

        private void Init()
        {
            var audioFormat = new AudioStreamBasicDescription()
            {
                SampleRate = this.SampleRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1,
                BitsPerChannel = this.BitsPerSample,
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0
            };

            audioQueue = new InputAudioQueue(audioFormat);
            audioQueue.InputCompleted += QueueInputCompleted;

            var bufferByteSize = this.bufferSize * audioFormat.BytesPerPacket;

            IntPtr bufferPtr;
            for (var index = 0; index < 3; index++)
            {
                audioQueue.AllocateBufferWithPacketDescriptors(bufferByteSize, this.bufferSize, out bufferPtr);
                audioQueue.EnqueueBuffer(bufferPtr, bufferByteSize, null);
            }
        }

        /// <summary>
        /// Handles iOS audio buffer queue completed message.
        /// </summary>
        /// <param name='sender'>Sender object</param>
        /// <param name='e'> Input completed parameters.</param>
        private void QueueInputCompleted(object sender, InputCompletedEventArgs e)
        {
            // return if we aren't actively monitoring audio packets
            if (!this.Active)
            {
                return;
            }

            var buffer = (AudioQueueBuffer)System.Runtime.InteropServices.Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
            if (this.OnBroadcast != null)
            {
                var send = new byte[buffer.AudioDataByteSize];
                System.Runtime.InteropServices.Marshal.Copy(buffer.AudioData, send, 0, (int)buffer.AudioDataByteSize);

                this.OnBroadcast(this, new EventArg<byte[]>(send));
            }

            var status = audioQueue.EnqueueBuffer(e.IntPtrBuffer, this.bufferSize, e.PacketDescriptions);

            if (status != AudioQueueStatus.Ok)
            {
                // todo: 
            }
        }

    }
}
