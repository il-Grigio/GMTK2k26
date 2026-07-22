using System;
using Grigios;
using UnityEngine;
using Random = UnityEngine.Random;

public class CityManager : Singleton<CityManager>
{
    [SerializeField] private GameObject[] BuildingsPrefab;
    [SerializeField] Transform player;
    [SerializeField] private float radius = 50;
    [SerializeField] private int distanceBetweenBuildings = 25;
    [SerializeField] private int streetWidth = 60;
    
    private int currentRowBuildingIndex = 0;

    private void Update()
    {
        while (player.position.z + radius > currentRowBuildingIndex * distanceBetweenBuildings)
        {
            currentRowBuildingIndex++;
            SpawnRow();
        }
    }

    private void SpawnRow()
    {
        SpawnBuildingLeft(new Vector3(streetWidth * 0.5f, 0, currentRowBuildingIndex * distanceBetweenBuildings));
        SpawnBuildingRight(new Vector3(-streetWidth * 0.5f, 0, currentRowBuildingIndex * distanceBetweenBuildings));
    }

    private void SpawnBuildingLeft(Vector3 position)
    {
        Instantiate(BuildingsPrefab[Random.Range(0, BuildingsPrefab.Length)], position, Quaternion.identity);
    }

    private void SpawnBuildingRight(Vector3 position)
    {
        Instantiate(BuildingsPrefab[Random.Range(0, BuildingsPrefab.Length)], position, Quaternion.identity);
    }
}
