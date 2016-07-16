using UnityEngine;
using System.Collections;

public class TestSlider : MenuSlider 
{
    public override void UpdateSliderValue()
    {
        HotValues.Instance().pointSensitivity = currentValue;
    }
}