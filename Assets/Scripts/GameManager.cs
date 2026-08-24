using Unity.Netcode;
using UnityEngine;

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
        GameObject ropeInstance = Instantiate(ropePrefab, Vector3.zero, Quaternion.identity);

        var networkObject = ropeInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
    }
}
