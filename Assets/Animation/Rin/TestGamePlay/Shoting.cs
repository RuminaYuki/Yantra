using UnityEngine;

public class Shoting : MonoBehaviour
{
    public GameObject ParticleObj;
    public Transform ShootPoint;

    public void Shoot()
    {
        if (ParticleObj != null)
        {
            GameObject bulletIns = Instantiate(ParticleObj, ShootPoint.position, ShootPoint.rotation);
        }
    }
}
