using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public Gun gun;

    // Sets the gun to upgraded mode when the player collides with the upgrade. Also removes the upgrade from the scene and plays a sound
    void OnTriggerEnter(Collider other)
    {
        gun.UpgradeGun();
        Destroy(gameObject);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.powerUpPickup);
    }
}
