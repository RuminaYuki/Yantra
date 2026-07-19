using UnityEngine;
public class Movement
{
    private readonly Rigidbody _rb;

    public float Speed { get; set; }

    public Movement(Rigidbody rb, float moveSpeed = 0)
    {
        _rb = rb;
        Speed = moveSpeed;
    }

    public void Move(Vector3 worldDirection)
    {
        worldDirection.y = 0f;
        worldDirection = Vector3.ClampMagnitude(
            worldDirection,
            1f);

        Vector3 targetVelocity =
            worldDirection * Speed;

        _rb.linearVelocity = new Vector3(
            targetVelocity.x,
            _rb.linearVelocity.y,
            targetVelocity.z);
    }

    public void Stop()
    {
        _rb.linearVelocity = new Vector3(
            0f,
            _rb.linearVelocity.y,
            0f);
    }
}