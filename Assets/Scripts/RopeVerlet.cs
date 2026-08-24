using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public class RopeVerlet : NetworkBehaviour
{
    [Header("Rope")]
    [SerializeField] private int numOfRopeSegments = 50;
    [SerializeField] private float ropeSegmentLength = 0.225f;

    [Header("Rope Physics")]
    [SerializeField] private Vector2 gravity = new Vector2(0, -2f);
    [SerializeField] private float damping = 0.98f;

    [Header("Constraints")]
    [SerializeField] private int constraintIterations = 50;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();

    private NetworkVariable<NetworkObjectReference> startPlayer = new NetworkVariable<NetworkObjectReference>();
    private NetworkVariable<NetworkObjectReference> endPlayer = new NetworkVariable<NetworkObjectReference>();
    private NetworkList<RopeSegmentState> replicatedSegments;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfRopeSegments;

        for (int i = 0; i < numOfRopeSegments; i++)
        {
            ropeSegments.Add(new RopeSegment(transform.position));
        }

        replicatedSegments = new NetworkList<RopeSegmentState>();
    }

    public override void OnNetworkSpawn()
    {
        replicatedSegments.OnListChanged += OnReplicatedSegmentsChanged;

        if (IsServer)
        {
            SetReplicatedSegments();
        }
    }

    public override void OnNetworkDespawn()
    {
        replicatedSegments.OnListChanged -= OnReplicatedSegmentsChanged;
    }

    public void AssignEndpoints(NetworkObject start, NetworkObject end)
    {
        if (!IsServer || start == null || end == null)
        {
            return;
        }

        startPlayer.Value = start;
        endPlayer.Value = end;

        Vector2 startPosition = start.transform.position;
        Vector2 endPosition = end.transform.position;
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            Vector2 position = Vector2.Lerp(startPosition, endPosition, i / (float)(ropeSegments.Count - 1));
            ropeSegments[i] = new RopeSegment(position);
        }

        SetReplicatedSegments();
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (!TryGetEndpoint(startPlayer.Value, out NetworkObject start) ||
            !TryGetEndpoint(endPlayer.Value, out NetworkObject end))
        {
            NetworkObject.Despawn();
            return;
        }

        SimulateRope();

        for (int i = 0; i < constraintIterations; i++)
        {
            ApplyConstraints(start.transform.position, end.transform.position);
        }

        SetReplicatedSegments();
    }

    private void Update()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[numOfRopeSegments];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            ropePositions[i] = ropeSegments[i].posNow;
        }

        lineRenderer.SetPositions(ropePositions);
    }

    private void SimulateRope()
    {
        // Simulate each segment
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            RopeSegment segment = ropeSegments[i];

            // Verlet integration
            Vector2 velocity = (segment.posNow - segment.posOld) * damping;
            segment.posOld = segment.posNow;
            segment.posNow += velocity + gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;

            ropeSegments[i] = segment;
        }
    }

    private void ApplyConstraints(Vector2 startPosition, Vector2 endPosition)
    {
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = startPosition;
        ropeSegments[0] = firstSegment;
        
        for(int i=0; i<numOfRopeSegments - 1; i++)
        {
            RopeSegment currentSeg = ropeSegments[i];
            RopeSegment nextSeg = ropeSegments[i + 1];

            float dist = (currentSeg.posNow - nextSeg.posNow).magnitude;
            float difference = (dist - ropeSegmentLength);

            Vector2 direction = (currentSeg.posNow - nextSeg.posNow).normalized;
            Vector2 changeVector = direction * difference;

            if (i != 0)
            {
                currentSeg.posNow -= changeVector * 0.5f;
                nextSeg.posNow += changeVector * 0.5f;
            }
            else
            {
                nextSeg.posNow += changeVector;
            }

            ropeSegments[i] = currentSeg;
            ropeSegments[i + 1] = nextSeg;  
        }

        RopeSegment lastSegment = ropeSegments[ropeSegments.Count - 1];
        lastSegment.posNow = endPosition;
        ropeSegments[ropeSegments.Count - 1] = lastSegment;
    }

    private bool TryGetEndpoint(NetworkObjectReference reference, out NetworkObject endpoint)
    {
        return reference.TryGet(out endpoint) && endpoint != null && endpoint.IsSpawned;
    }

    private void SetReplicatedSegments()
    {
        replicatedSegments.Clear();
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            replicatedSegments.Add(new RopeSegmentState(ropeSegments[i].posNow, ropeSegments[i].posOld));
        }
    }

    private void OnReplicatedSegmentsChanged(NetworkListEvent<RopeSegmentState> changeEvent)
    {
        if (IsServer || replicatedSegments.Count != ropeSegments.Count)
        {
            return;
        }

        for (int i = 0; i < ropeSegments.Count; i++)
        {
            RopeSegmentState state = replicatedSegments[i];
            ropeSegments[i] = new RopeSegment(state.posNow, state.posOld);
        }
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }

        public RopeSegment(Vector2 posNow, Vector2 posOld)
        {
            this.posNow = posNow;
            this.posOld = posOld;
        }
    }

    public struct RopeSegmentState : INetworkSerializable, IEquatable<RopeSegmentState>
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegmentState(Vector2 posNow, Vector2 posOld)
        {
            this.posNow = posNow;
            this.posOld = posOld;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref posNow);
            serializer.SerializeValue(ref posOld);
        }

        public bool Equals(RopeSegmentState other)
        {
            return posNow == other.posNow && posOld == other.posOld;
        }
    }
    
}
