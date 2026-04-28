using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] AudioSource TestSource;
    [SerializeField] CanvasGroup Canvas;
    [SerializeField] Slider Slider;
    private void Reset() {
        TestSource = GetComponent<AudioSource>();
        Canvas = GetComponent<CanvasGroup>();
        Slider = GetComponent<Slider>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Slider.value = PlayerPrefs.GetFloat("Volume",0.2f);
        HandleSlider(Slider.value);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Canvas.interactable = !Canvas.interactable;
            Canvas.blocksRaycasts = !Canvas.blocksRaycasts;
            Canvas.alpha = 0 == Canvas.alpha ? 1 : 0;
        }
    }
    public void HandleSlider(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume",value);
        TestSource.Play();
    }
}
