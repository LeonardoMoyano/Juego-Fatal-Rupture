using UnityEngine;

// Oculta el SpriteRenderer únicamente de los GameObjects que estén en el Layer "Wall".
// Así las paredes y puertas son invisibles pero mantienen sus colliders funcionales.
// El jugador, enemigos y cualquier otro objeto NO se ven afectados.
public class HideWallSprites : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HideSprites()
    {
        int wallLayer = LayerMask.NameToLayer("Wall");

        if (wallLayer == -1)
        {
            Debug.LogWarning("[HideWallSprites] No existe el Layer 'Wall'. Verificá que esté creado en Project Settings > Tags and Layers.");
            return;
        }

        SpriteRenderer[] renderers = Object.FindObjectsOfType<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            if (sr.gameObject.layer == wallLayer)
            {
                sr.enabled = false;
            }
        }
    }
}
