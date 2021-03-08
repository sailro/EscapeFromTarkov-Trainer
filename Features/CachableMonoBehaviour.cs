using UnityEngine;

namespace EFT.Trainer
{
	public abstract class CachableMonoBehaviour<T> : MonoBehaviour
	{
		public abstract float CacheTimeInSec { get; }
		public abstract bool Enabled { get; }

		private float _nextCacheTime;
		private T _data;

		public void Update()
		{
			if (!Enabled) 
				return;

			if (Time.time >= _nextCacheTime)
			{
				_data = RefreshData();
				_nextCacheTime = Time.time + CacheTimeInSec;
			}

			if (_data != null)
				ProcessData(_data);
		}

		public void OnGUI()
		{
			if (!Enabled) 
				return;

			if (_data != null)
				ProcessDataOnGUI(_data);
		}

		public abstract T RefreshData();
		public virtual void ProcessData(T data) { }
		public virtual void ProcessDataOnGUI(T data) { }
	}
}
