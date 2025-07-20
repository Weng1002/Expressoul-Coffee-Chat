using UnityEngine;
using System;
using System.IO;

public class AudioRecorder : MonoBehaviour
{
    private AudioClip recordedClip;
    private int sampleRate = 44100;
    private float saveInterval = 20f;
    private float nextSaveTime = 0f;
    private string micDevice;
    private string savePath = @"data\sound";
    private bool isRecording = false;
    public static string filepath;

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("找不到麥克風設備！");
            return;
        }

        micDevice = Microphone.devices[0];
        recordedClip = Microphone.Start(micDevice, true, 600, sampleRate);
        isRecording = true;
        nextSaveTime = Time.time + saveInterval;

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        Debug.Log("已開始錄音");
    }

    void Update()
    {
        if (!isRecording) return;

        if (Time.time >= nextSaveTime)
        {
            SaveLast20Seconds();
            nextSaveTime = Time.time + saveInterval;
        }
    }
    
    bool HasSound(AudioClip clip, float threshold = 0.01f)
{
    float[] samples = new float[clip.samples * clip.channels];
    clip.GetData(samples, 0);

    float sum = 0f;
    for (int i = 0; i < samples.Length; i++)
    {
        sum += samples[i] * samples[i];
    }

    float rms = Mathf.Sqrt(sum / samples.Length); // Root Mean Square 音量
    Debug.Log("RMS 音量：" + rms);

    return rms > threshold; // 超過門檻代表有聲音
}


    void SaveLast20Seconds()
    {
        int samplesLength = sampleRate * 20;
        int position = Microphone.GetPosition(micDevice);
        float[] samples = new float[samplesLength];
        int startSample = Mathf.Max(0, position - samplesLength);

        recordedClip.GetData(samples, startSample);
        AudioClip clipToSave = AudioClip.Create("TempClip", samplesLength, recordedClip.channels, sampleRate, false);
        clipToSave.SetData(samples, 0);

        SaveClipToWav(clipToSave);
    }

    void SaveClipToWav(AudioClip clip)
    {
        string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav";
        filepath = Path.Combine(savePath, filename);
        Debug.Log("儲存音訊至：" + filepath);

        byte[] data = ConvertClipToPCM16(clip);
        using (FileStream fs = new FileStream(filepath, FileMode.Create))
        {
            WriteWavHeader(fs, clip);
            fs.Write(data, 0, data.Length);
        }
        WavSender.record_already = true;
    }

    byte[] ConvertClipToPCM16(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            bytesData[i * 2] = byteArr[0];
            bytesData[i * 2 + 1] = byteArr[1];
        }

        return bytesData;
    }

    void WriteWavHeader(FileStream stream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(36 + samples * 2), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVEfmt "), 0, 8);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(hz), 0, 4);
        stream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}
