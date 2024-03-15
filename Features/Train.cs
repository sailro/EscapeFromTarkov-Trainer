using System;
using System.Diagnostics;
using EFT.MovingPlatforms;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Train : TriggerFeature
{
	public override string Name => "train";
	public override string Description => "Summon train on compatible maps like Reserve or Lighthouse.";

	public override KeyCode Key { get; set; } = KeyCode.None;

	protected override void UpdateOnceWhenTriggered()
	{
		var locomotive = FindObjectOfType<Locomotive>();
		if (locomotive == null)
			return;

		locomotive.Init(DateTime.UtcNow);
	}
}
