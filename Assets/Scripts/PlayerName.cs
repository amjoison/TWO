using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerName;

    public NetworkVariable<FixedString32Bytes> networkPlayerName = 
        new NetworkVariable<FixedString32Bytes>("Unknown", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public event Action<string> OnNameChanged;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            string inputName = FindFirstObjectByType<UIManager>().GetComponent<UIManager>().nameInputField.text;

            networkPlayerName.Value = new FixedString32Bytes(inputName);
            
        }
        playerName.text = networkPlayerName.Value.ToString();
        networkPlayerName.OnValueChanged += NetworkPlayerName_OnValueChanged;
        OnNameChanged?.Invoke(networkPlayerName.Value.ToString());
    }

    private void NetworkPlayerName_OnValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerName.text = newValue.Value;
        OnNameChanged?.Invoke(newValue.Value);
    }

    public string GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }
    
}
