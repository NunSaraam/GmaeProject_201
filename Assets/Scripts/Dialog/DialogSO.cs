using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogSO", menuName = "Dialog System/DialogSO")]
public class DialogSO : ScriptableObject
{
    public int id;
    public string characterName;
    public string text;
    public int nextId;

    public List<DialogChoiceSO> choises = new List<DialogChoiceSO>();
    public Sprite portrait;
    
    public string portaitPath;
}
