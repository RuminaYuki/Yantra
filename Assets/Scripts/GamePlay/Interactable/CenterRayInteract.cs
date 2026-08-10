using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class CenterRayInteract : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerRoot;

    [Header("Highlight Detection")]
    [SerializeField] private float highlightDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Focus Detection")]
    [SerializeField] private float focusDistance = 2f;
    [SerializeField, Range(0f, 1f)]
    private float focusScreenRadius = 0.15f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask obstacleLayer;

    public GameObject currentInteractableDebug;
    private Iinteractable currentInteractable;

    public Iinteractable CurrentInteractable => currentInteractable;

    private readonly Collider[] interactableBuffer = new Collider[32];

    private readonly HashSet<Iinteractable> highlightedInteractables = new();

    private void Update()
    {
        DetectNearbyInteractables();
        DetectInteractable();

        currentInteractableDebug = (currentInteractable as MonoBehaviour)?.gameObject;
    }

    /// <summary>
    /// หา Interactable รอบตัวเพื่อเปิด Highlight
    /// </summary>
    private void DetectNearbyInteractables()
    {
        int count = Physics.OverlapSphereNonAlloc(
            playerRoot.transform.position,
            highlightDistance,
            interactableBuffer,
            interactableLayer
        );

        HashSet<Iinteractable> detectedThisFrame = new();

        for (int i = 0; i < count; i++)
        {
            Collider collider = interactableBuffer[i];

            if (collider == null)
                continue;

            Iinteractable interactable =
                collider.GetComponentInParent<Iinteractable>();

            if (interactable == null)
                continue;

            detectedThisFrame.Add(interactable);

            if (!highlightedInteractables.Contains(interactable) && interactable.CanInteract)
            {
                interactable.ShowHighlight();
                highlightedInteractables.Add(interactable);
            }
        }

        // Remove highlights from interactables that are no longer detected
        foreach (Iinteractable interactable in highlightedInteractables)
        {
            if (!detectedThisFrame.Contains(interactable))
            {
                interactable.HideHighlight();
            }
        }

        highlightedInteractables.RemoveWhere(
            interactable => !detectedThisFrame.Contains(interactable)
        );
    }

    /// <summary>
    /// หา Interactable ที่อยู่ในหน้าจอและใกล้ Center
    /// </summary>
    private void DetectInteractable()
    {
        Iinteractable bestInteractable = null;

        float bestScore = float.MaxValue;

        foreach (Iinteractable interactable in highlightedInteractables)
        {
            if (interactable == null)
                continue;

            MonoBehaviour target = interactable as MonoBehaviour;

            if (target == null)
                continue;

            Vector3 targetPosition = target.transform.position;

            // ระยะจากกล้อง
            float distance =
                Vector3.Distance(
                    playerCamera.transform.position,
                    targetPosition
                );

            if (distance > focusDistance)
                continue;

            // แปลงตำแหน่งเป็น Viewport
            Vector3 viewportPosition =
                playerCamera.WorldToViewportPoint(targetPosition);

            // อยู่หลังกล้อง
            if (viewportPosition.z <= 0f)
                continue;

            // ระยะจาก Center Screen
            Vector2 screenCenter = new Vector2(0.5f, 0.5f);

            Vector2 targetScreenPosition =
                new Vector2(
                    viewportPosition.x,
                    viewportPosition.y
                );

            float screenDistance =
                Vector2.Distance(
                    screenCenter,
                    targetScreenPosition
                );

            // ไม่อยู่ใกล้ Center พอ
            if (screenDistance > focusScreenRadius)
                continue;

            // เช็คว่ามีสิ่งกีดขวางหรือไม่
            if (Physics.Linecast(
                playerCamera.transform.position,
                targetPosition,
                obstacleLayer))
            {
                continue;
            }

            // ยิ่งใกล้ Center ยิ่งได้คะแนนดี
            if (screenDistance < bestScore)
            {
                bestScore = screenDistance;
                bestInteractable = interactable;
            }
        }

        if (bestInteractable == null)
        {
            ClearCurrentInteractable();
            return;
        }

        SetFocus(bestInteractable);
    }

    private void SetFocus(Iinteractable interactable)
    {
        if (interactable == null || interactable == currentInteractable)
            return;

        ClearCurrentInteractable();

        currentInteractable = interactable;
        currentInteractable.OnFocus();
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable == null)
            return;

        currentInteractable.OnLoseFocus();
        currentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null || playerRoot == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            playerRoot.transform.position,
            highlightDistance
        );

        Gizmos.color = Color.red;

        Vector3 origin = playerCamera.transform.position;

        Gizmos.DrawRay(
            origin,
            playerCamera.transform.forward * focusDistance
        );
    }
}

public interface Iinteractable
{
    bool CanInteract { get; }

    //Command the object to perform its interaction logic
    void Interact(GameObject rootplayer);

    //Command the object to show Focus when in Camera forward
    void OnFocus();
    void OnLoseFocus();

    //Command the object to show Highlight when in Highlight range
    void ShowHighlight();
    void HideHighlight();
}