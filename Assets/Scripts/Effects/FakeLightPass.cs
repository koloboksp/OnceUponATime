using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightPass : ScriptableRenderPass
	{
		private RenderTargetHandle _destination;

		private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId(FakeLightingFeature.FakeLightTagName) };
		private FilteringSettings _filteringSettings;
		private RenderStateBlock _renderStateBlock;
		private int _downSample = 4;

		public Color FillColor;

		public FakeLightPass(RenderTargetHandle destination, int downSample)
		{
			_destination = destination;
			_downSample = downSample;

			_filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
			_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			var width = Mathf.Max(1, cameraTextureDescriptor.width >> _downSample);
			var height = Mathf.Max(1, cameraTextureDescriptor.height >> _downSample);
			var blurTextureDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0, 0);

			cmd.GetTemporaryRT(_destination.id, blurTextureDesc, FilterMode.Point);
			
			ConfigureTarget(_destination.Identifier());
			ConfigureClear(ClearFlag.Color, FillColor);		
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{		
			SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
			
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);		
		}
	}
}