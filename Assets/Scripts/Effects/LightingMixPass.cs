using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Effects
{
	public enum BufferType
	{
		CameraColor,
		Custom
	}

	public class LightingMixPass : ScriptableRenderPass
	{
		public FilterMode filterMode { get; set; }
		private string _profilerTag = "MixLightWith";
		private Material _material;

		private RenderTargetIdentifier _depth;

		RenderTargetIdentifier source;
		RenderTargetIdentifier destination;
		int temporaryRTId = Shader.PropertyToID("_TempRT");

		int sourceId;
		int destinationId;
		bool isSourceAndDestinationSameTarget;

		FakeLightingFeature.Settings settings;

		public LightingMixPass(FakeLightingFeature.Settings settings)
		{
			this.settings = settings;
		}


		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			blitTargetDescriptor.depthBufferBits = 0;

			isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
			                                   (settings.sourceType == BufferType.CameraColor || settings.sourceTextureId == settings.destinationTextureId);

			var renderer = renderingData.cameraData.renderer;

			if (settings.sourceType == BufferType.CameraColor)
			{
				sourceId = -1;
				source = renderer.cameraColorTarget;
			}
			else
			{
				sourceId = Shader.PropertyToID(settings.sourceTextureId);
				cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode);
				source = new RenderTargetIdentifier(sourceId);
			}

			if (isSourceAndDestinationSameTarget)
			{
				destinationId = temporaryRTId;
				cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
				destination = new RenderTargetIdentifier(destinationId);
			}
			else if (settings.destinationType == BufferType.CameraColor)
			{
				destinationId = -1;
				destination = renderer.cameraColorTarget;
			}
			else
			{
				destinationId = Shader.PropertyToID(settings.destinationTextureId);
				cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
				destination = new RenderTargetIdentifier(destinationId);
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

			if (isSourceAndDestinationSameTarget)
			{
				Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
				Blit(cmd, destination, source);
			}
			else
			{
				Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}		
	}
}