using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightingFeature : ScriptableRendererFeature
	{
		public Material FakeLightMaterial;
		FakeLightPass _pass;

		public override void Create()
		{

			_pass = new FakeLightPass(FakeLightMaterial);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(_pass);
		}
	}
}