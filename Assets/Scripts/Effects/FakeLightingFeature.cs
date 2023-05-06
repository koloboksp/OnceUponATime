using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
	public class FakeLightingFeature : ScriptableRendererFeature
	{
		public const string FakeLightTagName = "FakeLight";
		public const int FakeLightRenderTextureDownSample = 4;
		public const string FakeLightTextureName = "_FakeLightRenderTexture";
		public const RenderPassEvent FirePassEvent = RenderPassEvent.AfterRenderingOpaques;

		private RenderTargetHandle _fakeLightRenderTexture;
		private FakeLightPass _fakeLightPass;
		private LightingMixPass _lightingMixPass;

		[FormerlySerializedAs("settings")] [SerializeField] private Settings _settings;
		
		[NonSerialized]
		public bool Enabled = true;
		[NonSerialized]
		public Color FillLightColor = Color.gray;

		public override void Create()
		{
			_fakeLightRenderTexture.Init(FakeLightTextureName);
			
			_fakeLightPass = new FakeLightPass(_fakeLightRenderTexture, FakeLightRenderTextureDownSample);
			_fakeLightPass.renderPassEvent = FirePassEvent;

			_lightingMixPass = new LightingMixPass(_settings);
			_lightingMixPass.renderPassEvent = FirePassEvent;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (Enabled)
			{	
				_fakeLightPass.FillColor = FillLightColor;
				renderer.EnqueuePass(_fakeLightPass);
				renderer.EnqueuePass(_lightingMixPass);
			}
		}

		[System.Serializable]
		public class Settings
		{
			public Material blitMaterial = null;
			public int blitMaterialPassIndex = -1;
			public BufferType sourceType = BufferType.CameraColor;
			public BufferType destinationType = BufferType.CameraColor;
			public string sourceTextureId = "_SourceTexture";
			public string destinationTextureId = "_DestinationTexture";
		}
	}
}