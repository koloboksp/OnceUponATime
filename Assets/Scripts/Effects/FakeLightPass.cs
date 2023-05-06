using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightPass : ScriptableRenderPass
	{
		private RenderTargetHandle mDestination;

		private List<ShaderTagId> mShaderTagIdList = new List<ShaderTagId>() { new ShaderTagId(FakeLightingFeature.FakeLightTagName) };
		private FilteringSettings mFilteringSettings;
		private RenderStateBlock mRenderStateBlock;
		private int mDownSample = 4;

		public Color FillColor;

		public FakeLightPass(RenderTargetHandle destination, int downSample)
		{
			mDestination = destination;
			mDownSample = downSample;

			mFilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
			mRenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			var width = Mathf.Max(1, cameraTextureDescriptor.width >> mDownSample);
			var height = Mathf.Max(1, cameraTextureDescriptor.height >> mDownSample);
			var blurTextureDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBHalf, 0, 0);

			cmd.GetTemporaryRT(mDestination.id, blurTextureDesc, FilterMode.Point);
			
			ConfigureTarget(mDestination.Identifier());
			ConfigureClear(ClearFlag.Color, FillColor);		
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{		
			SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = CreateDrawingSettings(mShaderTagIdList, ref renderingData, sortingCriteria);
			
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref mFilteringSettings, ref mRenderStateBlock);		
		}
	}
}