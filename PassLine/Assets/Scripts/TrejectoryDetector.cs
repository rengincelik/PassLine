
// namespace PassLine.Assets.Scripts
// {
// using UnityEngine;

// public class TrajectoryRenderer : MonoBehaviour
// {
//     private LineRenderer lineRenderer;

//     private void Awake()
//     {
//         lineRenderer = GetComponent<LineRenderer>();

//         if (lineRenderer == null)
//         {
//             lineRenderer = gameObject.AddComponent<LineRenderer>();
//         }

//         SetupLineRenderer();
//     }

//     private void SetupLineRenderer()
//     {
//         lineRenderer.startWidth = 0.05f;
//         lineRenderer.endWidth = 0.05f;
//         lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
//         lineRenderer.startColor = new Color(1f, 1f, 0f, 0.8f);
//         lineRenderer.endColor = new Color(1f, 1f, 0f, 0.3f);
//         lineRenderer.enabled = false;
//     }

//     public void Show()
//     {
//         lineRenderer.enabled = true;
//     }

//     public void Hide()
//     {
//         lineRenderer.enabled = false;
//     }

//     public void UpdateTrajectory(Vector2 startPos, Vector2 velocity, int pointCount, float timeStep, float drag)
//     {
//         lineRenderer.positionCount = pointCount;

//         Vector2 currentPos = startPos;
//         Vector2 currentVel = velocity;

//         for (int i = 0; i < pointCount; i++)
//         {
//             lineRenderer.SetPosition(i, currentPos);
//             currentVel *= (1 - drag * timeStep);
//             currentPos += currentVel * timeStep;
//         }
//     }
// }
// }
