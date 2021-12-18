# Xamarin Forms iOS Audio Recording and Playing simulatneously in Linear PCM format

This repo is an example of how you can implement iOS Native racording and simultaneous playing in PCM format

What is PCM format?

>> Pulse-Code Modulation (PCM)is a digital audio file type that represents raw analog signals of the audio. PCM presents sound in waveforms. The waveforms are converted to digital bits through sampling and recording of sound at different intervals or pulses. The converted format features a sampling rate and the number of bits that represent a particular sample. PCM files are not compressed, and they are the closest thing to analog sound. The PCM audio file type is common among CDs. The sub-version of PCM is Linear Pulse-Code Modulation (LPCM), where samples are captured at linear intervals. It is the most popular PCM file type, hence the reason why people commonly interchange the terms.

- https://filewhopper.com/blog/what-audio-formats-are-the-best/

Why is there an echo when I test it with Speaker on?

Due to the slight delay in recording and playing, the mic is picking up the sound of the speaker causing an echo when played on speaker with a loud volume
