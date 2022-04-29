using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightingFeature : ScriptableRendererFeature
	{
		[System.Serializable]
		public class Settings
		{
			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

			public Material blitMaterial = null;
			public int blitMaterialPassIndex = -1;
			public BufferType sourceType = BufferType.CameraColor;
			public BufferType destinationType = BufferType.CameraColor;
			public string sourceTextureId = "_SourceTexture";
			public string destinationTextureId = "_DestinationTexture";
		}

		RenderTargetHandle _fakeLightTexture;

		[SerializeField]
		string _fakeLightTextureName;

		[SerializeField] public RenderPassEvent FakeLightPassEvent;

		[Space]
		public LayerMask LayerMask = 0;

		private FakeLightPass _fakeLightPass;


		MixLightingPass _mixLighting;
		public Material MixLighting = null;

		public Settings settings;
		public bool Enabled { get; set; }
		public Color FillLightColor;

		public override void Create()
		{
			_fakeLightTexture.Init(_fakeLightTextureName);
			
			_fakeLightPass = new FakeLightPass(_fakeLightTexture, LayerMask);
			_fakeLightPass.renderPassEvent = FakeLightPassEvent;
			_mixLighting = new MixLightingPass(settings);
			_mixLighting.renderPassEvent = FakeLightPassEvent;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			var colorTexture = renderer.cameraColorTarget;
			var depthTexture = renderer.cameraDepthTarget;
			_fakeLightPass.SetDepthTexture(depthTexture);

			_fakeLightPass.FillColor = FillLightColor;
			renderer.EnqueuePass(_fakeLightPass);
			renderer.EnqueuePass(_mixLighting);
		}
	}
}