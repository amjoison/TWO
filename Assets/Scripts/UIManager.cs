using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI gameInfoText;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] GameObject menu;

    void Start()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        menu.SetActive(false);
        gameInfoText.gameObject.SetActive(true);
        gameInfoText.text = "Press enter to start the game";
    }

    void OnJoinButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        menu.SetActive(false);
        gameInfoText.gameObject.SetActive(true);
        gameInfoText.text = "Waiting for host to start the game...";
    }
        
}
