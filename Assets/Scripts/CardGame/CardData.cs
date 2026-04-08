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
}
