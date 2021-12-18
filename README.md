# Xamarin Forms iOS Audio Recording and Playing simulatneously in Linear PCM format

This repo is an example of how you can implement iOS Native racording and simultaneous playing in PCM format.
This is challenging because it's difficult to play an audio from a changing stream of data, and raw PCM is not widely used

#### What is PCM format?

> Pulse-Code Modulation (PCM)is a digital audio file type that represents raw analog signals of the audio. PCM presents sound in waveforms. The waveforms are converted to digital bits through sampling and recording of sound at different intervals or pulses. The converted format features a sampling rate and the number of bits that represent a particular sample. PCM files are not compressed, and they are the closest thing to analog sound. The PCM audio file type is common among CDs. The sub-version of PCM is Linear Pulse-Code Modulation (LPCM), where samples are captured at linear intervals. It is the most popular PCM file type, hence the reason why people commonly interchange the terms.

-From https://filewhopper.com/blog/what-audio-formats-are-the-best/

#### Why is there an echo when I test it with Speaker on?

Due to the slight delay in recording and playing, the mic is picking up the sound of the speaker causing an echo when played on speaker with a loud volume

### Code explanation

Playing the recorded audio, we get the data in packets and construct the header of the wave (RIFF, WAVE, fmt, data) and then add the byte array to it
```
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
```

### Recording an audio stream in PCM format
```
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
```

### Interface in the Xamarin Forms project
```
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
```
