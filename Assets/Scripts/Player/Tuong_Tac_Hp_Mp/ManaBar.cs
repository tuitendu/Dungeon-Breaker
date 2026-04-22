using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private Slider manaSlider;

    public void SetMaxMana(int mana)
    {
        manaSlider.maxValue = mana;
        manaSlider.value = mana;
    }

    public void SetMana(int mana)
    {
        manaSlider.value = mana;
    }
}
