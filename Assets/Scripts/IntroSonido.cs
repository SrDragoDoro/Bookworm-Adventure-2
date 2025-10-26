using UnityEngine;
using System.Collections;

public class IntroSonido : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip v_welcometobookwormadventures;
    public AudioClip v_two;

    void Start()
    {
        StartCoroutine(ReproducirSonidosEnOrden());
    }

    IEnumerator ReproducirSonidosEnOrden()
    {
        //Reproduce el primer sonido
        audioSource.clip = v_welcometobookwormadventures;
        audioSource.Play();

        //Espera hasta que termine el primer sonido
        yield return new WaitForSeconds(v_welcometobookwormadventures.length - 1.28f);

        //Ahora reproduce el segundo
        audioSource.clip = v_two;
        audioSource.Play();
        
    }
}
