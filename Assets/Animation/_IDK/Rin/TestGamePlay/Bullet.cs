using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 20f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        MonsterAI monsterAI = collision.gameObject.GetComponent<MonsterAI>();
        if (monsterAI != null)
        {
            monsterAI.ApplyStunCooldown(); // Adjust the damage value as needed
        }
        Destroy(gameObject);
    }
}
