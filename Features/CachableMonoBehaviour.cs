using System.Collections;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class CachableMonoBehaviour<T> : MonoBehaviour, IEnableable
	{
		public abstract float CacheTimeInSec { get; }
		public abstract bool Enabled { get; set; }

		private T _data;
		private bool _refreshing = false;

		void Start()
		{
			StartCoroutine(RefreshDataScheduler());
		}

		private IEnumerator RefreshDataScheduler()
		{
			while (true)
			{
				if (Enabled)
				{
					try
					{
						_refreshing = true;
						/*if (PreloaderUI.Instantiated)
							PreloaderUI.Instance.Console.AddLog($"Refreshing {GetType().Name}...{DateTime.Now:hh:mm:ss}", "scheduler");*/
						_data = RefreshData();
					}
					finally
					{
						_refreshing = false;
					}
				}
				yield return new WaitForSeconds(CacheTimeInSec);
			}
		}

		public void Update()
		{
			if (!Enabled || _refreshing) 
				return;

			if (_data != null)
				ProcessData(_data);
		}

		public void OnGUI()
		{
			if (!Enabled || _refreshing) 
				return;

			if (_data != null)
				ProcessDataOnGUI(_data);
		}

		public abstract T RefreshData();
		public virtual void ProcessData(T data) { }
		public virtual void ProcessDataOnGUI(T data) { }
	}
}
