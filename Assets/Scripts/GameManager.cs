using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class GameManager : NetworkBehaviour
{
   public static GameManager Instance { get; private set; }
   [SerializeField] private GameObject ropePrefab;


   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(this.gameObject);
      }
      else
      {
         Instance = this;
      }
   }

   private void Update()
    {
        if (IsHost  && Input.GetKeyDown(KeyCode.Return))
        {
           Invoke(nameof(SpawnRope), 1f);
        }
    }

    private void SpawnRope()
    {
      if (!IsServer)
      {
         return;
      }

      NetworkObject[] players = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values
         .Where(networkObject => networkObject.IsSpawned && networkObject.GetComponent<PlayerMovement>() != null)
         .OrderBy(networkObject => networkObject.NetworkObjectId)
         .Take(2)
         .ToArray();

      if (players.Length < 2)
      {
         Debug.LogWarning("Cannot spawn the rope until two players are connected.");
         return;
      }

        GameObject ropeInstance = Instantiate(ropePrefab, Vector3.zero, Quaternion.identity);

        var networkObject = ropeInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();

         RopeVerlet rope = ropeInstance.GetComponent<RopeVerlet>();
         if (rope != null)
         {
            rope.AssignEndpoints(players[0], players[1]);
         }
        }
    }
}
