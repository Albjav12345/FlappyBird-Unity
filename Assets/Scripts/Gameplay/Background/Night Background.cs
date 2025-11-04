using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using JetBrains.Annotations;
using System;
using UnityEngine.Android;
using TMPro;
using Unity.VisualScripting;
using System.IO;

public class NightBackground : CustomBehaviourBase
{
    private string url = "https://api.sunrisesunset.io/json?";
    private bool hasLocationPermissions = false;

    [SerializeField] private float latitude;
    [SerializeField] private float longitude;

    private SunData sunData;

    [Header("Drag")]
    [SerializeField] SpriteRenderer nightBG;
    [SerializeField] SpriteRenderer nightBG1;

    [SerializeField] Animator nightBGAnimator;
    [SerializeField] Animator nightBGAnimator1;

    [HideInInspector] public int currentTime;
    [HideInInspector] public int intSunsetTime;
    [HideInInspector] public int intSunriseTime;

    private void Awake()
    {
        RequestLocationPermission();
        GameDataController.Instance.LoadGameData();
    }

    private void Update()
    {
        checkForLocationPermissions();
        GetCurrentTime();
        DetectNight();
    }

    private void RequestLocationPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }
    
    void checkForLocationPermissions()
    {
        if (!hasLocationPermissions)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                StartCoroutine(GetLocationData());
                hasLocationPermissions = true;
            }
            else
            {
                Debug.Log("Location permissions were not granted.");
            }
        }
    }

    private void GetCurrentTime()
    {
        DateTime holeDateData = DateTime.Now;
        string hora = holeDateData.ToString("HHmm");
        currentTime = int.Parse(hora);
    }

    private void DetectNight()
    {
        if (currentTime >= GameDataController.Instance.gameData.sunsetTime || currentTime <= GameDataController.Instance.gameData.sunriseTime)
        {
            ActivateNightBackground();
        }
        else
        {
            DeactivateNightBackground();
        }
    }

    private void ActivateNightBackground()
    {
        nightBGAnimator.SetTrigger("ActiveNightBgFadeIn");
        nightBGAnimator1.SetTrigger("ActiveNightBgFadeIn");
    }

    private void DeactivateNightBackground()
    {
        nightBGAnimator.SetTrigger("ActiveNightBgFadeOut");
        nightBGAnimator1.SetTrigger("ActiveNightBgFadeOut");
    }

    IEnumerator GetLocationData()
    {
        // Comprobar si la ubicación está habilitada en el dispositivo
        if (Input.location.isEnabledByUser)
        {
            // Iniciar servicio de ubicación
            Input.location.Start();

            // Esperar hasta que se obtenga una ubicación válida
            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return new WaitForSeconds(0);
            }

            if (Input.location.status == LocationServiceStatus.Running)
            {
                // Obtener latitud y longitud
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;

                Debug.Log("Tu posición es: " + latitude + ", " + longitude);

                // Detener el servicio de ubicación
                Input.location.Stop();

                // Llamar a la función para obtener los datos de la puesta de sol
                StartCoroutine(GetSunriseSunsetData());
            }
            else
            {
                Debug.Log("Error en la obtención de la ubicación.");
            }
        }
        else
        {
            Debug.Log("La ubicación no está habilitada en el dispositivo.");
        }
    }

    IEnumerator GetSunriseSunsetData()
    {
        // Formatea los valores de latitud y longitud con separador decimal punto
        string formattedLatitude = latitude.ToString("F6", CultureInfo.InvariantCulture);
        string formattedLongitude = longitude.ToString("F6", CultureInfo.InvariantCulture);

        string requestURL = url + "lat=" + formattedLatitude + "&lng=" + formattedLongitude;

        UnityWebRequest www = UnityWebRequest.Get(requestURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + www.error);
        }
        else
        {
            Debug.Log(requestURL);
            string jsonResponse = www.downloadHandler.text;
            sunData = JsonUtility.FromJson<SunData>(jsonResponse);

            ConvertSunsetTimeOnInteger();
            ConvertSunriseTimeOnInteger();
            GameDataController.Instance.SaveGameData();
        }
    }

    void ConvertSunsetTimeOnInteger()
    {
        string horaAMPM = sunData.results.sunset; // Hora en formato "6:33:22 PM"

        // Divide la cadena en horas, minutos, segundos y AM/PM
        string[] partes = horaAMPM.Split(new char[] { ' ', ':' });

        int hora24 = int.Parse(partes[0]);
        int minutos = int.Parse(partes[1]);

        if (partes[3] == "PM" && hora24 != 12)
        {
            hora24 += 12;
        }

        intSunsetTime = hora24 * 100 + minutos;

        GameDataController.Instance.gameData.sunsetTime = intSunsetTime;
    }

    void ConvertSunriseTimeOnInteger()
    {
        string horaAMPM = sunData.results.sunrise; // Hora en formato "6:33:22 PM"

        // Divide la cadena en horas, minutos, segundos y AM/PM
        string[] partes = horaAMPM.Split(new char[] { ' ', ':' });

        int hora24 = int.Parse(partes[0]);
        int minutos = int.Parse(partes[1]);

        if (partes[3] == "PM" && hora24 != 12)
        {
            hora24 += 12;
        }

        intSunriseTime = hora24 * 100 + minutos;

        GameDataController.Instance.gameData.sunriseTime = intSunriseTime;
    }
}