using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Componentes de la UI")]
    [SerializeField] private GameObject dialoguePanel; 
    [SerializeField] private TextMeshProUGUI textDisplay;   
    [SerializeField] private TextMeshProUGUI clickToContinueText; 

    [Header("Avatares (Sprites Cambiantes)")]
    [SerializeField] private Image leftAvatarImage;    
    [SerializeField] private Image rightAvatarImage;   

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

    // Referencia al nuevo Input System
    private InputSystem_Actions inputActions; 

    public bool IsDialogueActive { get; private set; }     

    private void Awake()
    {
        // 1. Inicializamos el Input System
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // 2. Habilitamos el mapa de acciones (Asumiremos que usaremos uno llamado "UI")
        inputActions.UI.Enable();

        // 3. Suscribimos nuestra función al evento 'performed' de la acción que crearemos
        // A esta acción la llamaremos "AdvanceDialogue" en Unity.
        inputActions.UI.AdvanceDialogue.performed += _ => OnPlayerInput();
    }

    private void OnDisable()
    {
        // 4. Desuscribimos el evento para evitar errores o memory leaks cuando el objeto se desactive
        inputActions.UI.AdvanceDialogue.performed -= _ => OnPlayerInput();

        // 5. Deshabilitamos el mapa de acciones
        inputActions.UI.Disable();
    }

    void Start()
    {
        dialoguePanel.SetActive(false);     
        clickToContinueText.gameObject.SetActive(false);  
        IsDialogueActive = false;     
    }

    void Update()
    {
        // El Update queda vacío porque los eventos del Input System ahora manejan la entrada.
    }

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

        typingCoroutine = StartCoroutine(TypeText(line.textContent));     
    }

    private IEnumerator TypeText(string targetText)     
    {
        isTyping = true;     
        textDisplay.text = "";     

        foreach (char letter in targetText.ToCharArray())     
        {
            textDisplay.text += letter;     

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
        textDisplay.text = currentSequence.lines[currentLineIndex].textContent;     
        clickToContinueText.gameObject.SetActive(true);     
    }

    // Renombramos OnPlayerClick a OnPlayerInput ya que ahora responde a teclado y mouse
    private void OnPlayerInput()
    {
        if (!IsDialogueActive) return;

        if (isTyping)   
        {
            StopCoroutine(typingCoroutine);  
            FinishLine();  
        }
        else 
        {
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
            EndDialogue();     
        }
    }

    private void EndDialogue()     
    {
        IsDialogueActive = false;     
        dialoguePanel.SetActive(false);     
        currentSequence = null;     
    }
}