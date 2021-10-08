using UnityEngine;
using UnityEngine.UI;

public class VolumePanel : CachedRectTransform
{
    [SerializeField] Sprite[] muteSprites;
    [SerializeField] Image btnMute;
    [SerializeField] Slider volumeSlider;

    bool muted;
    float lastValue;

    public System.Action<bool> OnMute;
    public System.Action<float> OnVolumeChange;

    public void ToggleMute()
    {
        muted = !muted;
        volumeSlider.value = muted ? 0 : lastValue;

        btnMute.sprite = muteSprites[muted ? 1 : 0];
        OnMute?.Invoke(muted);
    }

    public void VolumeChange()
    {
        OnVolumeChange?.Invoke(volumeSlider.value);
        lastValue = volumeSlider.value;

        if (muted)
        {
            if (volumeSlider.value > 0) ToggleMute();
            return;
        }

        if (volumeSlider.value <= 0) ToggleMute(); 
    }

    public void Init(float normalizedVolume, bool isMuted)
    {
        muted = isMuted;
        btnMute.sprite = muteSprites[muted ? 1 : 0];
        volumeSlider.value = lastValue = normalizedVolume;
    }

    public void Show() => LeanTween.moveY(MyTransform, 40, .25f).setEaseInSine();
    public void Hide() => LeanTween.moveY(MyTransform, -650, .25f).setEaseOutSine();
}
