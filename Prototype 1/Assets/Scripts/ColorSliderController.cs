using UnityEngine;
using UnityEngine.UI;

public class HueSliderController : MonoBehaviour
{
    public Slider hueSlider;
    public Image targetImage;  
    public Image previewBox;    

    void Start()
    {
        if (hueSlider != null)
            hueSlider.onValueChanged.AddListener(UpdateColor);

        UpdateColor(hueSlider.value); 
    }

    void UpdateColor(float value)
    {
        Color newColor = Color.HSVToRGB(value, 1f, 1f);

        if (targetImage != null)
            targetImage.color = newColor;
            
        if (previewBox != null)
            previewBox.color = newColor;
    }
}
