using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    public enum CardType
    {
        Attack,
        Heal,
        Buff,
        Utility,

    }


    public string cardName;
    public string description;
    public Sprite artwork;
    public int manaCost;
    public int effectAmount;
    public CardType cardType;

    public List<AdditionalEffect> addtionalEffects = new List<AdditionalEffect>();

    public enum AdditionalEffectType
    {
        None,
        DrawCard,
        DiscardCard,
        GainMana,
        ReduceEnemyMana,
        ReduceCardCost
    }

    public Color GetCardColor()
    {
        switch (cardType)
        {
            case CardType.Attack:
                return new Color(.9f, .3f, .3f);
            case CardType.Heal:
                return new Color(.3f, .9f, .3f);
            case CardType.Buff:
                return new Color(.9f, .3f, .9f);
            case CardType.Utility:
                return new Color(.9f, .9f, .3f);
            default:
                return Color.white;
        }
    }

    public string GetAdditionalEffectDescription()
    {
        if (addtionalEffects.Count == 0) return "";

        string result = "\n";

        foreach (var effect in addtionalEffects)
        {
            result += effect.GetDescription() + "\n";
        }

        return result;
    }
}
