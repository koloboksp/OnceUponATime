using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public class FakeLightPass : ScriptableRenderPass
	{
		string _profilerTag = "FakeLightPass";

		RenderTargetHandle _destination;
		RenderTargetIdentifier _depth;
		public FilterMode filterMode { get; set; }

		List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("FakeLight") };
		FilteringSettings _filteringSettings;
		RenderStateBlock _renderStateBlock;

		public Color FillColor;

		public FakeLightPass(RenderTargetHandle destination, int layerMask)
		{
			_destination = destination;

				
			_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
			_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		}

		public void SetDepthTexture(RenderTargetIdentifier depth)
		{
			_depth = depth;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cmd.GetTemporaryRT(_destination.id, cameraTextureDescriptor);
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