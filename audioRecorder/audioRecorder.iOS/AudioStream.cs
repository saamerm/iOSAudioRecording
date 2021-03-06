using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using audioRecorder.iOS;
using AudioToolbox;
using AVFoundation;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
[assembly: Dependency(typeof(AudioStream))]
namespace audioRecorder.iOS
{
    public class AudioStream : IAudioStream
    {
        private InputAudioQueue audioQueue;

        private readonly int _bufferSize;

        #region IAudioStream implementation

        public event EventHandler<EventArg<byte[]>> OnBroadcast;

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
            // Placed init here instead of the constructor because
            // i wasn't able to start again after stopping
            Init();
            return (audioQueue.Start() == AudioQueueStatus.Ok);
        }

        public void Stop()
        {
            audioQueue.Stop(true);
        }

        public void PlayByteArrayAsync(byte[] pcmData)
        {
            int numSamples = pcmData.Length / (BitsPerSample / 8);

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.ASCII);

            // Construct WAVE header.
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + sizeof(short) * numSamples);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });                // format chunk
            writer.Write(16);                                               // PCM chunk size
            writer.Write((short)1);                                         // PCM format flag
            writer.Write((short)ChannelCount);
            writer.Write(SampleRate);
            writer.Write(SampleRate * ChannelCount * BitsPerSample / 8);   // byte rate
            writer.Write((short)(ChannelCount * BitsPerSample / 8));         // block align
            writer.Write((short)BitsPerSample);
            writer.Write(new char[] { 'd', 'a', 't', 'a' });                // data chunk
            writer.Write(numSamples * ChannelCount * BitsPerSample / 8);

            // Write data as well.
            writer.Write(pcmData, 0, pcmData.Length);

            memoryStream.Seek(0, SeekOrigin.Begin);
            NSData data = NSData.FromStream(memoryStream);
            AVAudioPlayer audioPlayer = AVAudioPlayer.FromData(data);
            audioPlayer.Play();
        }

        public bool Active
        {
            get
            {
                return audioQueue.IsRunning;
            }
        }

        #endregion

        public AudioStream()
        {
            SampleRate = 8000;
            _bufferSize = 2048;
        }

        private void Init()
        {
            var audioFormat = new AudioStreamBasicDescription()
            {
                SampleRate = SampleRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1,
                BitsPerChannel = BitsPerSample,
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0
            };

            audioQueue = new InputAudioQueue(audioFormat);
            // Unassigns the event handler if assigned, or else does nothing
            // May not work to just put it in Stop() if the user taps Start() twice
            audioQueue.InputCompleted -= QueueInputCompleted;
            audioQueue.InputCompleted += QueueInputCompleted;

            var bufferByteSize = _bufferSize * audioFormat.BytesPerPacket;

            IntPtr bufferPtr;
            for (var index = 0; index < 3; index++)
            {
                audioQueue.AllocateBufferWithPacketDescriptors(bufferByteSize, _bufferSize, out bufferPtr);
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
            if (!Active)
            {
                return;
            }

            var buffer = (AudioQueueBuffer)System.Runtime.InteropServices.Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
            if (OnBroadcast != null)
            {
                var send = new byte[buffer.AudioDataByteSize];
                System.Runtime.InteropServices.Marshal.Copy(buffer.AudioData, send, 0, (int)buffer.AudioDataByteSize);

                OnBroadcast(this, new EventArg<byte[]>(send));
            }

            var status = audioQueue.EnqueueBuffer(e.IntPtrBuffer, _bufferSize, e.PacketDescriptions);

            if (status != AudioQueueStatus.Ok)
            {
                // todo: 
            }
        }

    }
}
