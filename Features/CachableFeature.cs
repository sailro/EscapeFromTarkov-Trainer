using System.Collections;
using EFT.Trainer.Configuration;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal abstract class CachableFeature<T> : ToggleFeature
	{
		[ConfigurationProperty(Order = 3)]
		public abstract float CacheTimeInSec { get; set; }

		private T? _data;
		private bool _refreshing = false;

#if DEBUG_PERFORMANCE
		private readonly System.Diagnostics.Stopwatch _stopwatch = new();
#endif

		[UsedImplicitly]
		private void Start()
		{
			StartCoroutine(RefreshDataScheduler());
		}

		private IEnumerator RefreshDataScheduler()
		{
			if (Enabled)
			{
				try
				{
					_refreshing = true;

#if DEBUG_PERFORMANCE
					_stopwatch.Restart();
					AddConsoleLog($"Refreshing {GetType().Name}...");
#endif

					_data = RefreshData();
				}
				finally
				{
					_refreshing = false;

#if DEBUG_PERFORMANCE
					_stopwatch.Stop();
#endif

				}
			}

#if DEBUG_PERFORMANCE
			AddConsoleLog($"Refreshed in {_stopwatch.ElapsedMilliseconds}ms...");
#endif

			yield return new WaitForSeconds(CacheTimeInSec);
			StartCoroutine(RefreshDataScheduler());
		}

		protected override void UpdateWhenEnabled()
		{
			if (_refreshing) 
				return;

			if (_data != null)
				ProcessData(_data);
		}

		protected override void OnGUIWhenEnabled()
		{
			if (_refreshing) 
				return;

			if (_data != null)
				ProcessDataOnGUI(_data);
		}

		public abstract T? RefreshData();
		public virtual void ProcessData(T data) { }
		public virtual void ProcessDataOnGUI(T data) { }
	}
}
