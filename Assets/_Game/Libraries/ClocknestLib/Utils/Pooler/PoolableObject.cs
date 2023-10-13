// Extension of Corgi Engine's MMPoolableObject. 
// Credits to More Mountain.

using UnityEngine;
using System.Collections;
using System;
using ClocknestGames.Game.Core;
using UnityEngine.SceneManagement;

namespace ClocknestGames.Library.Utils
{
	/// <summary>
	/// Add this class to an object that you expect to pool from an objectPooler. 
	/// Note that these objects can't be destroyed by calling Destroy(), they'll just be set inactive (that's the whole point).
	/// </summary>
	public class PoolableObject : MonoBehaviour
	{
		public delegate void Events();
		public event Events OnSpawnComplete;

		[Header("Poolable Object")]
		/// The life time, in seconds, of the object. If set to 0 it'll live forever, if set to any positive value it'll be set inactive after that time.
		public float LifeTime = 0f;

		public Transform InitialParent { get; set; }

		/// <summary>
		/// Turns the instance inactive, in order to eventually reuse it.
		/// </summary>
		public virtual void Destroy()
		{
			if (InitialParent != null)
				transform.SetParent(InitialParent.transform);

			gameObject.SetActive(false);
		}

		public virtual void Execute() { }

		/// <summary>
		/// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (LifeTime > 0f)
			{
				Invoke("Destroy", LifeTime);
			}
		}

		/// <summary>
		/// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}

		/// <summary>
		/// Triggers the on spawn complete event
		/// </summary>
		public virtual void TriggerOnSpawnComplete()
		{
			OnSpawnComplete?.Invoke();
		}
	}
}
