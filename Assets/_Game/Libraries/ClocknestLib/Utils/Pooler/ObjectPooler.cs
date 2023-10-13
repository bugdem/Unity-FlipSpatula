// Extension of Corgi Engine's MMObjectPooler. 
// Credits to More Mountain.

using UnityEngine;
using System.Collections.Generic;

namespace ClocknestGames.Library.Utils
{
	/// <summary>
	/// A base class, meant to be extended depending on the use (simple, multiple object pooler), and used as an interface by the spawners.
	/// Still handles common stuff like singleton and initialization on start().
	/// DO NOT add this class to a prefab, nothing would happen. Instead, add SimpleObjectPooler or MultipleObjectPooler.
	/// </summary>
	public abstract class ObjectPooler : MonoBehaviour, EventListener<SceneEvent>
	{
		/// singleton pattern
		public static ObjectPooler Instance;
		/// if this is true, the pool will try not to create a new waiting pool if it finds one with the same name.
		public bool MutualizeWaitingPools = false;
		/// if this is true, all waiting and active objects will be regrouped under an empty game object. Otherwise they'll just be at top level in the hierarchy
		public bool NestWaitingPool = true;
		public bool NestCurrentGameobject = false;
		/// for performance considers, set this manually if object's itself or parent is do not destroy on load.
		public bool IsDontDestroyOnLoadObject = true;

		/// this object is just used to group the pooled objects
		protected GameObject _waitingPool = null;
		protected ObjectPool _objectPool;

		/// <summary>
		/// On awake we fill our object pool
		/// </summary>
		protected virtual void Awake()
		{
			Instance = this;
			FillObjectPool();
		}

		/// <summary>
		/// Creates the waiting pool or tries to reuse one if there's already one available
		/// </summary>
		protected virtual void CreateWaitingPool()
		{
			if (!NestWaitingPool)
			{
				return;
			}

			if (NestCurrentGameobject)
			{
				_waitingPool = gameObject;
				_objectPool = _waitingPool.AddComponent<ObjectPool>();
				_objectPool.PooledGameObjects = new List<PoolableObject>();
				return;
			}

			if (!MutualizeWaitingPools)
			{
				// we create a container that will hold all the instances we create
				_waitingPool = new GameObject(DetermineObjectPoolName());
				_objectPool = _waitingPool.AddComponent<ObjectPool>();
				_objectPool.PooledGameObjects = new List<PoolableObject>();
				return;
			}
			else
			{
				GameObject waitingPool = GameObject.Find(DetermineObjectPoolName());
				if (waitingPool != null)
				{
					_waitingPool = waitingPool;
					_objectPool = _waitingPool.GetComponentNoAlloc<ObjectPool>();
				}
				else
				{
					_waitingPool = new GameObject(DetermineObjectPoolName());
					_objectPool = _waitingPool.AddComponent<ObjectPool>();
					_objectPool.PooledGameObjects = new List<PoolableObject>();
				}
			}
		}

		private void DestroyPoolObjects()
		{
			var poolObjects = GetPoolObjects();
			if (poolObjects != null && poolObjects.Count > 0)
			{
				foreach (var obj in poolObjects)
				{
					var poolableObj = obj.GetComponent<PoolableObject>();
					if (poolableObj != null)
						poolableObj.Destroy();
				}
			}
		}

		/// <summary>
		/// Gets poolable objects from pool to be used on scene unload event, which destroys used poolable objects.
		/// </summary>
		/// <returns>The pool objects.</returns>
		protected abstract List<PoolableObject> GetPoolObjects();

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected virtual string DetermineObjectPoolName()
		{
			return ("[ObjectPooler] " + this.name);
		}

		/// <summary>
		/// Implement this method to fill the pool with objects
		/// </summary>
		public virtual void FillObjectPool()
		{
			return;
		}

		/// <summary>
		/// Implement this method to return a gameobject
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public virtual PoolableObject GetPooledGameObject()
		{
			return null;
		}

		/// <summary>
		/// Destroys the object pool
		/// </summary>
		public virtual void DestroyObjectPool()
		{
			if (_waitingPool != null)
			{
				Destroy(_waitingPool.gameObject);
			}
		}

		public virtual void ObjectAdded(PoolableObject poolableObj)
		{
			if (poolableObj != null)
				poolableObj.InitialParent = poolableObj.transform.parent;
		}

		private void OnEnable()
		{
			if (IsDontDestroyOnLoadObject)
				this.EventStartListening<SceneEvent>();
		}

		private void OnDisable()
		{
			if (IsDontDestroyOnLoadObject)
				this.EventStopListening<SceneEvent>();
		}

		public void OnCGEvent(SceneEvent currentEvent)
		{
			if (currentEvent.EventType == SceneEventType.Unloaded)
				DestroyPoolObjects();
		}
	}
}