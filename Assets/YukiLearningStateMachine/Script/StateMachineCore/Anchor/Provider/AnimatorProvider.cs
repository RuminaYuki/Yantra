using UnityEngine;

public class AnimatorProvider : MonoBehaviour
{
    [SerializeField] private AnimatorAnchor animatorAnchor;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animatorAnchor == null)
        {
            Debug.LogError("AnimatorAnchor is missing.", this);
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator is missing.", this);
            return;
        }

        animatorAnchor.Provide(animator);
    }

    private void OnDisable()
    {
        if (animatorAnchor != null)
            animatorAnchor.Unset();
    }
}