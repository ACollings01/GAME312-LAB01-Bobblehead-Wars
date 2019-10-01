using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;               // Represents the space marine
    public GameObject[] spawnPoints;        // Different locations aliens can spawn
    public GameObject alien;                // Alien prefab

    public int maxAliensOnScreen;           // Max number of aliens allowed on screen at one time
    public int totalAliens;                 // Total number of aliens to spawn over the course of one game
    public float minSpawnTime;              // Controls the rate at which aliens spawn
    public float maxSpawnTime;              // Controls the rate at which aliens spawn
    public int aliensPerSpawn;              // How many aliens appear with each spawn event

    private int aliensOnScreen = 0;         // Holds the number of aliens currently on screen
    private float generatedSpawnTime = 0;   // Tracks time between spawn events
    private float currentSpawnTime = 0;     // Holds time since last spawn

    public GameObject upgradePrefab;        // Holds the prefab for the upgrades
    public Gun gun;
    public float upgradeMaxTimeSpawn = 7.5f;

    private bool spawnedUpgrade = false;
    private float actualUpgradeTime = 0;
    private float currentUpgradeTime = 0;

    public GameObject deathFloor;

    public Animator arenaAnimator;

    // Start is called before the first frame update
    void Start()
    {
        actualUpgradeTime = Random.Range(upgradeMaxTimeSpawn - 3.0f,
                                            upgradeMaxTimeSpawn);
        actualUpgradeTime = Mathf.Abs(actualUpgradeTime);
    }

    // Update is called once per frame
    void Update()
    {
        // Exit if the player is dead
        if (player == null)
        {
            return;
        }

        currentUpgradeTime += Time.deltaTime;   // Updates the current upgrade time with the time passed each frame

        if (currentUpgradeTime > actualUpgradeTime)
        {
            if (!spawnedUpgrade)
            {
                // Generates a random spawn location of all the spawns
                int randomNumber = Random.Range(0, spawnPoints.Length - 1);
                GameObject spawnLocation = spawnPoints[randomNumber];

                // Creates the upgrade object in that location
                GameObject upgrade = Instantiate(upgradePrefab) as GameObject;
                Upgrade upgradeScript = upgrade.GetComponent<Upgrade>();
                upgradeScript.gun = gun;
                upgrade.transform.position = spawnLocation.transform.position;

                // Lets the game know there is an upgrade created
                spawnedUpgrade = true;
            }
        }

        currentSpawnTime += Time.deltaTime; // Updates the current spawn time with the time passed each frame

        if (currentSpawnTime > generatedSpawnTime)  // Resets the counter and generates a new random interval for the next spawn
        {
            currentSpawnTime = 0;
            generatedSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);

            if (aliensPerSpawn > 0 && aliensOnScreen < totalAliens) // Determines whether or not to spawn
            {
                List<int> previousSpawnLocations = new List<int>(); // Creates a list of spawn points
                if (aliensPerSpawn > spawnPoints.Length) // Limits the number of aliens able to spawn to the number of available spawn points
                {
                    aliensPerSpawn = spawnPoints.Length - 1;
                }

                aliensPerSpawn = (aliensPerSpawn > totalAliens) ? aliensPerSpawn - totalAliens : aliensPerSpawn;    // Prevents the game from spawnwing more aliens than totalAliens allows

                for (int i = 0; i < aliensPerSpawn; i++)    // Runs an amount of times equal to how many aliens we want to spawn
                {
                    if (aliensOnScreen < maxAliensOnScreen) // Never spawns more than the max allowed on screen
                    {
                        aliensOnScreen += 1;                // Increment the number of aliens on screen

                        int spawnPoint = -1;                // Reset spawn point, find a random spawn for the alien
                        while (spawnPoint == -1)
                        {
                            int randomNumber = Random.Range(0, spawnPoints.Length - 1);

                            if (!previousSpawnLocations.Contains(randomNumber))
                            {
                                previousSpawnLocations.Add(randomNumber);       // Adds the spawn point to the array if it is not yet in there
                                spawnPoint = randomNumber;
                            }
                        }

                        GameObject spawnLocation = spawnPoints[spawnPoint];
                        GameObject newAlien = Instantiate(alien) as GameObject;

                        newAlien.transform.position = spawnLocation.transform.position; // Sets the position of the new alien

                        // Sets the target for the new alien to the position of the player.
                        Alien alienScript = newAlien.GetComponent<Alien>();
                        alienScript.target = player.transform;

                        // Turns the alien to face the player.
                        Vector3 targetRotation = new Vector3(player.transform.position.x,
                                                                newAlien.transform.position.y,
                                                                player.transform.position.z);
                        newAlien.transform.LookAt(targetRotation);

                        alienScript.OnDestroy.AddListener(AlienDestroyed);

                        alienScript.GetDeathParticles().SetDeathFloor(deathFloor);
                    }
                }
            }
        }
    }

    public void AlienDestroyed()
    {
        aliensOnScreen -= 1;
        totalAliens -= 1;

        if (totalAliens == 0)
        {
            Invoke("endGame", 2.0f);
        }
    }

    private void endGame()
    {
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.elevatorArrived);
        arenaAnimator.SetTrigger("PlayerWon");
    }
}
