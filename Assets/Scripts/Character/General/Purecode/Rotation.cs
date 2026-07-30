using UnityEngine;
public class Rotation
{
    private readonly Rigidbody _rb;

    public float Speed { get; set; }

    public Rotation(Rigidbody rb, float speed = 0)
    {
        _rb = rb;
        Speed = speed;
    }

    public void Rotate(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        Quaternion nextRotation = Quaternion.Slerp(_rb.rotation,
            targetRotation,Speed * deltaTime);

        _rb.MoveRotation(nextRotation);
    }
}