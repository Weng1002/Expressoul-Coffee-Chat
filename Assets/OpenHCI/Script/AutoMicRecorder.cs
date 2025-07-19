using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MicroPhoneInput : MonoBehaviour
{
    private static MicroPhoneInput m_instance;
    private AudioClip clip;

    private string saveFolderPath = @"F:\\unity\\OpenHCI_V\\data\\sound\\";
    public float sensitivity = 100;
    public float loudness = 0;

    private static string[] micArray = null;
    const int HEADER_SIZE = 44;
    const int RECORD_TIME = 10;

    void Start()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            Debug.Log("Created directory: " + saveFolderPath);
        }
        StartRecord();
    }

    public static MicroPhoneInput getInstance()
    {
        if (m_instance == null)
        {
            micArray = Microphone.devices;
            if (micArray.Length == 0)
            {
                Debug.LogError("Microphone.devices is null");
            }
            foreach (string deviceStr in Microphone.devices)
            {
                Debug.Log("device name = " + deviceStr);
            }
            if (micArray.Length == 0)
            {
                Debug.LogError("no mic device");
            }

            GameObject MicObj = new GameObject("MicObj");
            m_instance = MicObj.AddComponent<MicroPhoneInput>();
        }
        return m_instance;
    }

    public void StartRecord()
    {
        GetComponent<AudioSource>().Stop();
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("No Record Device!");
            return;
        }
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().mute = true;
        GetComponent<AudioSource>().clip = Microphone.Start(null, false, RECORD_TIME, 44100);
        clip = GetComponent<AudioSource>().clip;
        StartCoroutine(WaitThenStop());
    }

    private IEnumerator WaitThenStop()
    {
        yield return new WaitForSeconds(RECORD_TIME + 0.1f);
        StopRecord();
        string filename = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string fullPath = Path.Combine(saveFolderPath, filename);
        Save(fullPath, clip);
        Debug.Log("Recording saved to: " + fullPath);
    }

    public void StopRecord()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            GetComponent<AudioSource>().Stop();
            Debug.Log("StopRecord");
        }
    }

    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        string filepath = filename;
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (FileStream fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }

        return true;
    }

    static FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        fileStream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(fileStream.Length - 8), 0, 4);
        fileStream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        fileStream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
        fileStream.Write(BitConverter.GetBytes(16), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        fileStream.Write(BitConverter.GetBytes(hz), 0, 4);
        fileStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        fileStream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}
