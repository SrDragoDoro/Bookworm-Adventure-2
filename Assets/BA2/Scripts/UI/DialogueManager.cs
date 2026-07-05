using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Componentes de la UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private TextMeshProUGUI clickToContinueText; // "(Click para continuar)"

    [Header("Avatares (Sprites Cambiantes)")]
    [SerializeField] private Image leftAvatarImage;  // Contenedor de Lex (Izquierda)
    [SerializeField] private Image rightAvatarImage; // Contenedor de NPC (Derecha)

    [Header("Configuración de Texto")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Audio (Máquina de escribir)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typewriterSounds;

    // Estados de control
    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private InputSystem_Actions inputActions; //Se debe usar para que la tecla Escape, espacio, enter e intro hagan lo mismo que el click del mouse, o sea, avanzar el diálogo.


    // Propiedad pública para que otros scripts consulten o controlen la activación activa
    public bool IsDialogueActive { get; private set; }

    void Start()
    {
        // Aseguramos que empiece oculto
        dialoguePanel.SetActive(false);
        clickToContinueText.gameObject.SetActive(false);
        IsDialogueActive = false;
    }

    void Update()
    {
        // Si no está activo, ignorar inputs
        if (!IsDialogueActive) return;

        // Detectar click en pantalla (puedes cambiarlo por un botón transparente gigante en la UI)
        if (Input.GetMouseButtonDown(0))
        {
            OnPlayerClick();
        }
    }

    
    // Activa e inicia una secuencia de diálogos desde cualquier script externo. 
    public void StartDialogue(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines.Length == 0) return;

        currentSequence = sequence;
        currentLineIndex = 0;
        IsDialogueActive = true;
        dialoguePanel.SetActive(true);

        ShowLine(currentSequence.lines[currentLineIndex]);
    }

    private void ShowLine(DialogueLine line)
    {
        textDisplay.text = "";
        clickToContinueText.gameObject.SetActive(false);

        // Configuración de Layout simétrica al juego original
        if (line.isLex)
        {
            leftAvatarImage.gameObject.SetActive(true);
            rightAvatarImage.gameObject.SetActive(false);
            leftAvatarImage.sprite = line.speakerSprite;
        }
        else
        {
            leftAvatarImage.gameObject.SetActive(false);
            rightAvatarImage.gameObject.SetActive(true);
            rightAvatarImage.sprite = line.speakerSprite;
        }

        // Iniciar efecto máquina de escribir
        typingCoroutine = StartCoroutine(TypeText(line.textContent));
    }

    private IEnumerator TypeText(string targetText)
    {
        isTyping = true;
        textDisplay.text = "";

        foreach (char letter in targetText.ToCharArray())
        {
            textDisplay.text += letter;

            // Reproducir sonido aleatorio de la lista por cada letra
            if (typewriterSounds.Length > 0 && audioSource != null)
            {
                AudioClip randomClip = typewriterSounds[Random.Range(0, typewriterSounds.Length)];
                audioSource.PlayOneShot(randomClip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        FinishLine();
    }

    private void FinishLine()
    {
        isTyping = false;
        textDisplay.text = currentSequence.lines[currentLineIndex].textContent; // Asegurar texto completo
        clickToContinueText.gameObject.SetActive(true); // Mostrar indicador omitir/continuar
    }

    private void OnPlayerClick()
    {
        if (isTyping)
        {
            // PRIMER CLICK: Detener escritura y autocompletar el párrafo actual
            StopCoroutine(typingCoroutine);
            FinishLine();
        }
        else
        {
            // SEGUNDO CLICK: Avanzar al siguiente diálogo
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < currentSequence.lines.Length)
        {
            ShowLine(currentSequence.lines[currentLineIndex]);
        }
        else
        {
            // Fin de la conversación
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;
        dialoguePanel.SetActive(false);
        currentSequence = null;
        // Aquí puedes disparar un evento de C# si necesitas avisar al combate que continúe
    }
}