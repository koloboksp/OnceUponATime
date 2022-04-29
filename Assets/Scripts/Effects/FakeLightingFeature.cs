using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightingFeature : ScriptableRendererFeature
	{
		public const string FakeLightTagName = "FakeLight";
		public const int FakeLightRenderTextureDownSample = 4;
		public const string FakeLightTextureName = "_FakeLightRenderTexture";
		public const RenderPassEvent FirePassEvent = RenderPassEvent.AfterRenderingOpaques;

		RenderTargetHandle _FakeLightRenderTexture;
		FakeLightPass mFakeLightPass;
		LightingMixPass mLightingMixPass;

		public Settings settings;
		
		[NonSerialized]
		public bool Enabled = true;
		[NonSerialized]
		public Color FillLightColor = Color.gray;

		public override void Create()
		{
			
			_FakeLightRenderTexture.Init(FakeLightTextureName);
			
			mFakeLightPass = new FakeLightPass(_FakeLightRenderTexture, FakeLightRenderTextureDownSample);
			mFakeLightPass.renderPassEvent = FirePassEvent;

			mLightingMixPass = new LightingMixPass(settings);
			mLightingMixPass.renderPassEvent = FirePassEvent;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (Enabled)
			{	
				mFakeLightPass.FillColor = FillLightColor;
				renderer.EnqueuePass(mFakeLightPass);
				renderer.EnqueuePass(mLightingMixPass);
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