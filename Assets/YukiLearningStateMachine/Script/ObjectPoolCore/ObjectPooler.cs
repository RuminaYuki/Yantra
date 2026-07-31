using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : Singleton<ObjectPooler>
{

    protected override bool UseDontDestroyOnLoad => false;
	
    public ObjectPoolTableSO poolTable;
	public bool useScriptableObject = true;
	public List<Pool> pools;
	private List<Pool> finalPools;
    public Dictionary<string, List<GameObject>> poolDictionary;


	protected override void Awake()
	{
		base.Awake();

		finalPools = new List<Pool>();

		if (pools != null)
			finalPools.AddRange(pools);

		if (useScriptableObject && poolTable != null && poolTable.poolTableList != null)
		{
			foreach (var listSO in poolTable.poolTableList)
			{
				if (listSO == null || listSO.pools == null)
					continue;

				foreach (var tablePool in listSO.pools)
				{
					var existing = finalPools.Find(p => p.tag == tablePool.tag);

					if (existing == null)
						finalPools.Add(tablePool);
					else
						existing.size += tablePool.size;
				}
			}
		}


		poolDictionary = new Dictionary<string, List<GameObject>>();

		foreach (Pool pool in finalPools)
		{
			List<GameObject> objectPool = new List<GameObject>();

			for (int i = 0; i < pool.size; i++)
			{
				GameObject obj = Instantiate(pool.prefab);
				obj.SetActive(false);
				objectPool.Add(obj);
			}

			poolDictionary.Add(pool.tag, objectPool);
		}
	}
	public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, System.Action<GameObject> beforeSpawn = null)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
			return null;
		}

		List<GameObject> pool = poolDictionary[tag];

		GameObject objectToSpawn = null;

		foreach (GameObject obj in pool)
		{
			if (!obj.activeInHierarchy)
			{
				objectToSpawn = obj;
				break;
			}
		}

		if (objectToSpawn == null)
		{
			if (objectToSpawn == null)
			{
				Pool poolConfig = finalPools.Find(p => p.tag == tag);

				if (poolConfig == null)
				{
					Debug.LogWarning("No pool config found for tag: " + tag);
					return null;
				}

				objectToSpawn = Instantiate(poolConfig.prefab);
				objectToSpawn.SetActive(false);
				pool.Add(objectToSpawn);
			}
		}

		objectToSpawn.transform.position = position;
		objectToSpawn.transform.rotation = rotation;

		beforeSpawn?.Invoke(objectToSpawn);

		objectToSpawn.SetActive(true);

		IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
		if (pooledObj != null)
		{
			pooledObj.OnObjectSpawn();
		}

		return objectToSpawn;
	}
}