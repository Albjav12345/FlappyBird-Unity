using UnityEngine;
using TMPro;
using CarterGames.Assets.AudioManager;
using RDG;

public class NicknameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;

    public void OnSaveButtonClicked()
    {
        AudioManager.Play("pop");
        Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_HEAVY_CLICK);
        
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("El nombre no puede estar vacío.");
            return;
        }

        // Guardar en tu sistema de GameData
        GameDataController.Instance.gameData.nickname = nickname;
        GameDataController.Instance.SaveGameData();

        Debug.Log("Nombre guardado: " + nickname);

        // Llamar al GameManager para continuar
        GameManager.Instance.OnNicknameSaved();
    }
}