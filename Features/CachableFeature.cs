using System.Collections;
using System.Collections.Generic;
using EFT.Trainer.Configuration;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal abstract class CachableFeature<T> : ToggleFeature
{
	[ConfigurationProperty(Order = 3)]
	public abstract float CacheTimeInSec { get; set; }

	private readonly List<T> _data = [];
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
#endif

				BeforeRefreshData(_data);
				_data.Clear(); // but we'll keep previous capacity
				RefreshData(_data);
			}
			finally
			{
				_refreshing = false;

#if DEBUG_PERFORMANCE
				_stopwatch.Stop();
#endif

			}

#if DEBUG_PERFORMANCE
			AddConsoleLog($"Refreshed {GetType().Name} in {_stopwatch.ElapsedMilliseconds}ms");
#endif
		}

		yield return new WaitForSeconds(CacheTimeInSec);
		StartCoroutine(RefreshDataScheduler());
	}

	protected override void UpdateWhenEnabled()
	{
		if (_refreshing) 
			return;

		if (_data.Count > 0)
			ProcessData(_data);
	}

	protected override void OnGUIWhenEnabled()
	{
		if (_refreshing) 
			return;

		if (_data.Count > 0)
			ProcessDataOnGUI(_data);
	}

	protected virtual void BeforeRefreshData(IReadOnlyList<T> data) { }
	public abstract void RefreshData(List<T> data);
	public virtual void ProcessData(IReadOnlyList<T> data) { }
	public virtual void ProcessDataOnGUI(IReadOnlyList<T> data) { }
}
