using UnityEngine;

namespace EFT.Trainer.Features;

internal class ShaderProperties
{
	public static readonly int FirstOutlineColor = Shader.PropertyToID("_FirstOutlineColor");
	public static readonly int FirstOutlineWidth = Shader.PropertyToID("_FirstOutlineWidth");
	public static readonly int SecondOutlineColor = Shader.PropertyToID("_SecondOutlineColor");
	public static readonly int SecondOutlineWidth = Shader.PropertyToID("_SecondOutlineWidth");
	public static readonly int ZTest = Shader.PropertyToID("_ZTest");
}
