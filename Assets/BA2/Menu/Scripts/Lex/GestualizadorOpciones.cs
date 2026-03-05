using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GestualizadorOpciones : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioSource audioSource;
    public AudioClip hoverSound;

    public Image personaje;       // La imagen del personaje en el Canvas
    public Sprite spriteNormal;   // Imagen normal
    public Sprite spriteHablando1; // Imagen “hablando” Gesto oh
    public Sprite spriteHablando2; // Imagen “hablando” Gesto eh
    public Sprite spriteHablando3; // Imagen “hablando” Gesto oh
    public Sprite spriteHablando4; // Imagen “hablando” Gesto eh

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Reproduce sonido
        audioSource.PlayOneShot(hoverSound);

        // Cambia sprite
        personaje.sprite = spriteHablando1;
        personaje.sprite = spriteHablando2;
        personaje.sprite = spriteHablando3;
        personaje.sprite = spriteHablando4;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Vuelve al sprite normal
        personaje.sprite = spriteNormal;
    }
}
