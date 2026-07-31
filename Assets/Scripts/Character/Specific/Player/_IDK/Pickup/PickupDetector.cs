using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PickupDetector : MonoBehaviour
{
    [System.Serializable]
    public class PickupItemEvent : UnityEvent<PickupItem>
    {
    }

    [Header("Raycast")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField] private string pickupTag = "Pickup";

    [Header("Input")]
    [SerializeField] private Key pickupKey = Key.E;
    [SerializeField] private string pickupKeyDisplayName = "E";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultPromptText = "Press {0} to pick up";

    [Header("Pickup Result")]
    [SerializeField] private bool hideObjectOnPickup = true;
    [SerializeField] private PickupItemEvent onPickup = new PickupItemEvent();

    private PickupItem currentItem;
    private GameObject currentPickupObject;

    private void Awake()
    {
        ResolveRaycastCamera();
        HidePrompt();
    }

    private void OnDisable()
    {
        currentItem = null;
        currentPickupObject = null;
        HidePrompt();
    }

    private void Update()
    {
        UpdateCurrentPickupTarget();

        if (currentPickupObject == null)
        {
            return;
        }

        if (pickupKey == Key.None || Keyboard.current == null)
        {
            return;
        }

        var keyControl = Keyboard.current[pickupKey];
        if (keyControl != null && keyControl.wasPressedThisFrame)
        {
            PickupCurrentItem();
        }
    }

    private void UpdateCurrentPickupTarget()
    {
        PickupTarget target = FindPickupTargetInView();
        if (target.PickupObject == currentPickupObject)
        {
            return;
        }

        currentItem = target.Item;
        currentPickupObject = target.PickupObject;

        if (currentPickupObject == null)
        {
            HidePrompt();
            return;
        }

        ShowPrompt(currentItem);
    }

    private PickupTarget FindPickupTargetInView()
    {
        ResolveRaycastCamera();

        if (raycastCamera == null)
        {
            return default;
        }

        Ray ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance, raycastLayers, QueryTriggerInteraction.Collide))
        {
            return default;
        }

        Transform pickupTransform = FindTaggedPickupTransform(hit.collider.transform);
        if (pickupTransform == null)
        {
            return default;
        }

        PickupItem item = pickupTransform.GetComponentInParent<PickupItem>()
                          ?? pickupTransform.GetComponentInChildren<PickupItem>();

        GameObject pickupObject = item != null ? item.gameObject : pickupTransform.gameObject;
        return new PickupTarget(item, pickupObject);
    }

    private Transform FindTaggedPickupTransform(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            if (current.CompareTag(pickupTag))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private void PickupCurrentItem()
    {
        GameObject pickedObject = currentPickupObject;
        if (pickedObject == null)
        {
            return;
        }

        OnPickupPressed(currentItem, pickedObject);

        if (currentItem != null)
        {
            onPickup?.Invoke(currentItem);
        }

        if (hideObjectOnPickup)
        {
            pickedObject.SetActive(false);
        }

        currentItem = null;
        currentPickupObject = null;
        HidePrompt();
    }

    private void OnPickupPressed(PickupItem item, GameObject pickedObject)
    {
        // TODO: Friend implements inventory or item logic here.
    }

    private void ShowPrompt(PickupItem item)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = item != null
            ? item.GetPromptText(pickupKeyDisplayName)
            : string.Format(defaultPromptText, pickupKeyDisplayName);
        promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(false);
    }

    private void ResolveRaycastCamera()
    {
        if (raycastCamera != null)
        {
            return;
        }

        raycastCamera = Camera.main;
        if (raycastCamera != null)
        {
            return;
        }

        raycastCamera = FindFirstObjectByType<Camera>();
    }

    private readonly struct PickupTarget
    {
        public readonly PickupItem Item;
        public readonly GameObject PickupObject;

        public PickupTarget(PickupItem item, GameObject pickupObject)
        {
            Item = item;
            PickupObject = pickupObject;
        }
    }
}
