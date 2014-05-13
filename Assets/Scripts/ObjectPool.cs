using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

    public static ObjectPool instance;
    public GameObject[] objectPrefabs;
    public List<GameObject>[] pooledObjects;
    public int[] amountToBuffer;
    protected GameObject containerObject;

    void Awake() {
        instance = this;
    }

	void Start() {
        containerObject = new GameObject("ObjectPool");
        pooledObjects = new List<GameObject>[objectPrefabs.Length];
        int i = 0;
        for(int j = 0; j < objectPrefabs.Length; j++) {
            pooledObjects[i] = new List<GameObject>();  
            int bufferAmount = amountToBuffer[i];
            for(int n = 0; n < bufferAmount; n++) {
                GameObject newObj = Instantiate(objectPrefabs[j]) as GameObject;
                newObj.name = objectPrefabs[j].name;
                PoolObject(newObj);
            }
            i++;
        }
	}
	
	public GameObject GetObjectForType(string objectType, bool onlyPooled) {
        for(int i = 0; i < objectPrefabs.Length; i++) {
            GameObject prefab = objectPrefabs[i];
            if(prefab.name == objectType) {
                if(pooledObjects[i].Count > 0) {
                    GameObject pooledObject = pooledObjects[i][0];
                    pooledObjects[i].RemoveAt(0);
                    pooledObject.transform.parent = null;
                    pooledObject.SetActive(true);
                    return pooledObject;
                } else if(!onlyPooled) {
                    return Instantiate(objectPrefabs[i]) as GameObject;
                }
                break;
            }
        }
        return null;
    }

    public void PoolObject(GameObject obj) {
        for(int i = 0; i < objectPrefabs.Length; i++) {
            if(objectPrefabs[i].name == obj.name) {
                obj.SetActive(false);
                obj.transform.parent = containerObject.transform;
                pooledObjects[i].Add(obj);
                return;
            }
        }
    }

}
