using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Pool<T> where T : Component
{
    [SerializeField] private T _prefab;
    private List<T> _pooledObjects = new List<T>();
    private Transform _parent;
    public void Init(int initialSize)
    {
        string nameOfPool = _prefab.name + " Pool";
        _parent = new GameObject(nameOfPool).transform;
        for(int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pooledObjects.Add(obj);
        }
    }

    public T Get()
    {
        T obj = _pooledObjects.FirstOrDefault(o => !o.gameObject.activeSelf);
        if(obj == null)
        {
            obj = Object.Instantiate(_prefab, _parent);
            _pooledObjects.Add(obj);
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
    }       
}
