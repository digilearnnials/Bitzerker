using UnityEngine;
using UnityEngine.Events;

public class AutomaticSlider : MonoBehaviour
{
    [SerializeField] private bool autoReverse = false;

    [SerializeField] private AnimationCurve sliderBehaviour = default;
    
    [SerializeField, Min(0.01f)] float duration = 1f;

    [SerializeField] OnValueChangedEvent onValueChanged = default;
    
    float value;

    public bool Reversed { get; set; }
    
    public bool AutoReverse {
        get => autoReverse;
        set => autoReverse = value;
    }
    
    float SmoothedValue => sliderBehaviour.Evaluate(value);
    
    void FixedUpdate () {
        float delta = Time.deltaTime / duration;
        if (Reversed) {
            value -= delta;
            if (value <= 0f) {
                if (autoReverse) {
                    value = Mathf.Min(1f, -value);
                    Reversed = false;
                }
                else {
                    value = 0f;
                    enabled = false;
                }
            }
        }
        else {
            value += delta;
            if (value >= 1f) {
                if (autoReverse) {
                    value = Mathf.Max(0f, 2f - value);
                    Reversed = true;
                }
                else {
                    value = 1f;
                    enabled = false;
                }
            }
        }
        onValueChanged.Invoke(sliderBehaviour.Evaluate(value));
    }
}

[System.Serializable]
public class OnValueChangedEvent : UnityEvent<float> { }
