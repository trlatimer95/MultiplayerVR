using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Toggle snapTurnToggle;
    [SerializeField] private Toggle teleportToggle;
    [SerializeField] private Toggle vignetteToggle;
    [SerializeField] private Slider moveSpeedSlider;

    [Header("Controllers")]
    [SerializeField] private GameObject rightHandUIRay;
    [SerializeField] private GameObject rightHandTeleportRay;

    [Header("Vignette Object")]
    [SerializeField] private GameObject vignette;

    [Header("Network")]
    [SerializeField] private NetworkManager networkManager;

    private ActionBasedSnapTurnProvider snapTurnProvider;
    private ActionBasedContinuousTurnProvider continuousTurnProvider;
    private ActionBasedContinuousMoveProvider continousMoveProvider;
    private TeleportationProvider teleportationProvider;

    private void Start()
    {
        GameObject playerRig = GameObject.FindGameObjectWithTag("Player");
        snapTurnProvider = playerRig.GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurnProvider = playerRig.GetComponent<ActionBasedContinuousTurnProvider>();
        continousMoveProvider = playerRig.GetComponent<ActionBasedContinuousMoveProvider>();
        teleportationProvider = playerRig.GetComponent<TeleportationProvider>();
        networkManager = FindObjectOfType<NetworkManager>();

        if (continuousTurnProvider == null)
            Debug.Log("No Continuous Turn Provider found");

        if (snapTurnProvider == null)
            Debug.Log("No snap turn provider found");

        if (continousMoveProvider == null)
            Debug.Log("No continuous move provider found");

        if (teleportationProvider == null)
            Debug.Log("No teleportation provider found");

        LoadSettings();
        snapTurnToggle.isOn = snapTurnProvider.enabled;
        teleportToggle.isOn = teleportationProvider.enabled;
        vignetteToggle.isOn = vignette.activeInHierarchy;
    }

    public void ToggleSnapTurn(bool toggleState)
    {
        snapTurnProvider.enabled = toggleState;
        continuousTurnProvider.enabled = !toggleState;
    }

    public void ToggleTeleport(bool toggleState)
    {
        teleportationProvider.enabled = toggleState;
        rightHandTeleportRay.SetActive(toggleState);
        continousMoveProvider.enabled = !toggleState;
        rightHandUIRay.SetActive(!toggleState);
    }

    public void ToggleVignette(bool toggleState)
    {
        vignette.SetActive(toggleState);
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeedSlider.value = speed;
        continousMoveProvider.moveSpeed = speed;
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LeaveRoom()
    {
        if (networkManager != null)
            networkManager.Disconnect();
        else
            Debug.Log("No NetworkManager found");
    }

    public void QuitGame()
    {
#if DEBUG
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region Saving/Loading
    [System.Serializable]
    class SettingsData
    {
        public bool UseSnapTurn;
        public bool UseTeleport;
        public bool UseVignette;
        public float MoveSpeed;
    }

    public void SaveSettings()
    {
        SettingsData settings = new SettingsData();
        settings.UseSnapTurn = snapTurnToggle.isOn;
        settings.UseTeleport = teleportToggle.isOn;
        settings.UseVignette = vignetteToggle.isOn;
        settings.MoveSpeed = moveSpeedSlider.value;

        string json = JsonUtility.ToJson(settings);

        File.WriteAllText(Application.persistentDataPath + "/settings.json", json);
    }

    public void LoadSettings()
    {
        string path = Application.persistentDataPath + "/settings.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SettingsData settings = JsonUtility.FromJson<SettingsData>(json);

            ToggleSnapTurn(settings.UseSnapTurn);
            ToggleTeleport(settings.UseTeleport);
            ToggleVignette(settings.UseVignette);
            SetMoveSpeed(settings.MoveSpeed);
        }
    }
    #endregion
}


