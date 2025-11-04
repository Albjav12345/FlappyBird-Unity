using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leaderboardContent; // Content del ScrollView
    [SerializeField] private GameObject playerNamePrefab;  // Prefab Player Name Box

    private void Awake()
    {
        if (leaderboardContent == null)
            Debug.LogError("❌ Falta asignar el Content del ScrollView en LeaderboardUI.");

        if (playerNamePrefab == null)
            Debug.LogError("❌ Falta asignar el prefab Player Name Box en LeaderboardUI.");
    }
    
    private void Start()
    {
        List<ScoreData> testScores = new List<ScoreData>()
        {
            new ScoreData("Alice", 10, ""),
            new ScoreData("Bob", 8, ""),
            new ScoreData("Carlos", 6, "")
        };

        Populate(testScores);
    }

    /// <summary>
    /// Limpia la UI actual de entradas
    /// </summary>
    public void ClearEntries()
    {
        foreach (Transform child in leaderboardContent)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Crea las entradas de leaderboard dinámicamente
    /// </summary>
    public void Populate(List<ScoreData> scores)
    {
        ClearEntries();

        if (scores == null || scores.Count == 0)
        {
            Debug.LogWarning("⚠ No hay datos de puntuaciones para mostrar en UI.");
            return;
        }

        Debug.Log($"🧱 Generando {scores.Count} elementos de leaderboard en UI...");

        for (int i = 0; i < scores.Count; i++)
        {
            var entry = scores[i];

            // Instanciamos prefab
            GameObject newEntry = Instantiate(playerNamePrefab, leaderboardContent);
            newEntry.name = $"Entry_{i + 1}_{entry.name}";

            // Buscamos el componente de texto
            var text = newEntry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{i + 1}. {entry.name} — {entry.score}";
                Debug.Log($"✅ Añadido a UI: {text.text}");
            }
            else
            {
                Debug.LogError($"❌ No se encontró componente TextMeshProUGUI en el prefab '{playerNamePrefab.name}'.");
            }
        }

        Debug.Log("✅ LeaderboardUI actualizado correctamente.");
    }
}
