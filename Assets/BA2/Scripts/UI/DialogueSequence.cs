using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    public bool isLex; // TRUE: Izquierda (Lex) | FALSE: Derecha (NPC)
    [TextArea(3, 5)] public string textContent;
    public Sprite speakerSprite;
}

[CreateAssetMenu(fileName = "NuevoDialogo", menuName = "Bookworm Remake/Secuencia de Diálogo")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] lines;
}