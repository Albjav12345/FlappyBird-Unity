using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance; // singleton simple para acceder desde otros scripts
    DatabaseReference dbReference;
    FirebaseAuth auth;

    void Awake() {
        // Singleton pattern simple
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Si estamos compilando, salimos inmediatamente
#if UNITY_EDITOR
        // Evita inicializar Firebase mientras se hace el build
        if (UnityEditor.BuildPipeline.isBuildingPlayer)
        {
            Debug.Log("Saltando inicialización de Firebase durante el build");
            return;
        }
#endif

        // Inicializar Firebase y auth
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) {
                Debug.Log("Firebase OK");
                auth = FirebaseAuth.DefaultInstance;
                // Iniciar sign-in anónimo (opcional, pero recomendado)
                auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask => {
                    if (authTask.IsCompleted && !authTask.IsFaulted) {
                        Debug.Log("Anon sign-in OK: " + auth.CurrentUser.UserId);
                        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                    } else {
                        Debug.LogWarning("Auth failed, still trying DB without auth");
                        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                    }
                });
            } else {
                Debug.LogError("Fallo dependencias: " + task.Result);
            }
        });
    }

    // SendScore con callback opcional de éxito/fracaso
    public void SendScore(string playerName, int score, Action<bool> callback = null)
    {
        if (dbReference == null) {
            Debug.LogError("DB no inicializada");
            callback?.Invoke(false);
            return;
        }

        string key = dbReference.Child("scores").Push().Key;
        ScoreData data = new ScoreData(playerName, score, DateTime.UtcNow.ToString("o"));
        string json = JsonUtility.ToJson(data);

        dbReference.Child("scores").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(t => {
            if (t.IsCompleted && !t.IsFaulted) {
                Debug.Log("Score guardada");
                callback?.Invoke(true);
            } else {
                Debug.LogError("Error guardando score: " + t.Exception);
                callback?.Invoke(false);
            }
        });
    }

    // GetScores con un callback que recibe la lista ordenada
    public void GetTopScores(int limit, Action<List<ScoreData>> callback)
    {
        if (dbReference == null)
        {
            Debug.LogError("DB no inicializada");
            callback?.Invoke(new List<ScoreData>());
            return;
        }

        dbReference.Child("scores").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            List<ScoreData> leaderboard = new List<ScoreData>();

            if (task.IsFaulted)
            {
                Debug.LogError("❌ Error leyendo scores: " + task.Exception);
                callback?.Invoke(leaderboard);
                return;
            }

            if (task.IsCompleted && task.Result != null)
            {
                DataSnapshot snap = task.Result;
                Debug.Log("📊 Se leyeron " + snap.ChildrenCount + " registros de Firebase.");

                foreach (var child in snap.Children)
                {
                    string name = child.Child("name").Value?.ToString() ?? "Anon";
                    int.TryParse(child.Child("score").Value?.ToString() ?? "0", out int s);
                    string time = child.Child("time")?.Value?.ToString() ?? "";
                    leaderboard.Add(new ScoreData(name, s, time));
                }

                // 🔹 Ordenar de mayor a menor (ya que no usamos OrderByChild)
                leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

                // 🔹 Limitar al número pedido
                if (leaderboard.Count > limit)
                    leaderboard = leaderboard.GetRange(0, limit);
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo obtener snapshot de Firebase.");
            }

            callback?.Invoke(leaderboard);
        });
    }

}

[Serializable]
public class ScoreData {
    public string name;
    public int score;
    public string time;

    public ScoreData(string name, int score, string time) {
        this.name = name;
        this.score = score;
        this.time = time;
    }
}
