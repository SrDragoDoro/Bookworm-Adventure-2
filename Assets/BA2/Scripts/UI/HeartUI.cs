using UnityEngine;

public class HeartUI : MonoBehaviour
{
    [Tooltip("Arrastra los 4 sprites del corazón. \nÍndice 0: Último pedazo en perderse (1 HP).\nÍndice 3: Primer pedazo en perderse (4 HP).")]
    public SpriteRenderer[] piecesOrder = new SpriteRenderer[4];

    [Tooltip("Prefab temporal que caerá cuando se reciba daño")]
    public GameObject fallingPiecePrefab;

    private int activePieces = 4;

    // activePiecesAmount va de 0 a 4
    public void UpdatePieces(int targetActivePieces)
    {
        for (int i = 0; i < 4; i++)
        {
            bool shouldBeActive = i < targetActivePieces;

            // Si estaba activo y ahora no debe estarlo (recibió daño)
            if (piecesOrder[i].gameObject.activeSelf && !shouldBeActive)
            {
                TriggerFallingPiece(piecesOrder[i]);
            }

            // Aplicamos el estado final (se oculta o se muestra si hubo curación)
            piecesOrder[i].gameObject.SetActive(shouldBeActive);
        }

        activePieces = targetActivePieces;
    }

    private void TriggerFallingPiece(SpriteRenderer originalPiece)
    {
        if (fallingPiecePrefab == null) return;

        // Instanciamos el fragmento que caerá exactamente donde estaba el original
        GameObject fallingClone = Instantiate(fallingPiecePrefab, originalPiece.transform.position, originalPiece.transform.rotation);

        // Le asignamos el mismo sprite
        SpriteRenderer cloneRenderer = fallingClone.GetComponent<SpriteRenderer>();
        if (cloneRenderer != null) cloneRenderer.sprite = originalPiece.sprite;

        // Le damos un pequeño empuje aleatorio para que se vea dinámico
        Rigidbody2D rb = fallingClone.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomX = Random.Range(-2f, 2f);
            rb.AddForce(new Vector2(randomX, 3f), ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-50f, 50f));
        }

        // Destruimos el fragmento físico después de 3 segundos para limpiar la escena
        Destroy(fallingClone, 3f);
    }
}