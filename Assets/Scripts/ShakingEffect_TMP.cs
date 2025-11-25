using UnityEngine;
using TMPro;

public class ShakingEffect_TMP : MonoBehaviour
{
    public TMP_Text textComponent;

    [Header("Shake Settings")]
    public float shakeIntensity = 1.5f; // qué tanto se mueven las letras
    public float shakeFrequency = 25f;  // qué tan rápido tiemblan

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();

        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Un "ruido" distinto para cada letra usando su índice
            float noiseX = (Mathf.PerlinNoise(i * 0.3f, Time.time * shakeFrequency) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(Time.time * shakeFrequency, i * 0.3f) - 0.5f) * 2f;

            Vector3 offset = new Vector3(noiseX, noiseY, 0f) * shakeIntensity;

            for (int j = 0; j < 4; ++j)
            {
                verts[charInfo.vertexIndex + j] += offset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
