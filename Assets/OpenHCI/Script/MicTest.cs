using UnityEngine;

public class MicTest : MonoBehaviour
{
    private AudioClip clip;
    private string micName;
    private float recordDuration = 10f; // 錄音時間設定為 10 秒

    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log($"麥克風裝置: {device}");
        }

        micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (string.IsNullOrEmpty(micName))
        {
            Debug.LogError("找不到麥克風");
            return;
        }

        Debug.Log("開始錄音");

        clip = Microphone.Start(micName, false, (int)recordDuration, 16000);
        Invoke(nameof(StopTestRecording), recordDuration);
    }

    void StopTestRecording()
    {
        Microphone.End(micName);
        Debug.Log("錄音結束");

        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        float maxVolume = 0f;
        foreach (var sample in samples)
        {
            if (Mathf.Abs(sample) > maxVolume)
                maxVolume = Mathf.Abs(sample);
        }

        Debug.Log($"音訊最大音量: {maxVolume}");

        if (maxVolume < 0.01f)
        {
            Debug.LogWarning("音訊太小，可能沒有聲音被錄到");
        }
        else
        {
            Debug.Log("🎙 有錄到聲音！");
        }
    }
}
