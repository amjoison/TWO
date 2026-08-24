using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class RopeVerlet : MonoBehaviour
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

    private Vector3 ropeStartPoint;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfRopeSegments;

        ropeStartPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        for (int i = 0; i < numOfRopeSegments; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegmentLength;
        }
    }

    private void FixedUpdate()
    {
        SimulateRope();

        for (int i = 0; i < constraintIterations; i++)
        {
            ApplyConstraints();
        }
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

    private void ApplyConstraints()
    {
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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
    }
    
}
