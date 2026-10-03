using System.Collections;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Pedestrian")]
    [SerializeField] private GameObject pedestrianPrefab;

    [Header("Routes")]
    [SerializeField] private PedestrianRoute[] routes;

    [Header("Spawn Settings")]
    [SerializeField] private int maxPedestrians = 5;
    [SerializeField] private float spawnInterval = 3f;

    private int activePedestrians;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (activePedestrians >= maxPedestrians)
                continue;

            SpawnPedestrian();
        }
    }

    private void SpawnPedestrian()
    {
        if (pedestrianPrefab == null ||
            routes == null ||
            routes.Length == 0)
        {
            return;
        }

        PedestrianRoute route =
            routes[Random.Range(0, routes.Length)];

        Transform spawnPoint = route.GetPoint(0);

        if (spawnPoint == null)
            return;

        GameObject pedestrian = Instantiate(
            pedestrianPrefab,
            spawnPoint.position + Vector3.up * 0.95f,
            spawnPoint.rotation
        );

        PedestrianController controller =
            pedestrian.GetComponent<PedestrianController>();

        if (controller != null)
        {
            controller.SetRoute(route, this);
        }

        activePedestrians++;
    }

    public void PedestrianDestroyed()
    {
        activePedestrians = Mathf.Max(0, activePedestrians - 1);
    }
}