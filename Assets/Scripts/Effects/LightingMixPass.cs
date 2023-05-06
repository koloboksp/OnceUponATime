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
		private string _profilerTag = "MixLightWith";
		private Material _material;

		private RenderTargetIdentifier _depth;

		private RenderTargetIdentifier _source;
		private RenderTargetIdentifier _destination;
		private int _temporaryRTId = Shader.PropertyToID("_TempRT");

		private int _sourceId;
		private int _destinationId;
		private bool _isSourceAndDestinationSameTarget;

		private FakeLightingFeature.Settings _settings;

		public FilterMode FilterMode { get; set; }
		
		public LightingMixPass(FakeLightingFeature.Settings settings)
		{
			this._settings = settings;
		}


		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			blitTargetDescriptor.depthBufferBits = 0;

			_isSourceAndDestinationSameTarget = _settings.sourceType == _settings.destinationType &&
			                                   (_settings.sourceType == BufferType.CameraColor || _settings.sourceTextureId == _settings.destinationTextureId);

			var renderer = renderingData.cameraData.renderer;

			if (_settings.sourceType == BufferType.CameraColor)
			{
				_sourceId = -1;
				_source = renderer.cameraColorTarget;
			}
			else
			{
				_sourceId = Shader.PropertyToID(_settings.sourceTextureId);
				cmd.GetTemporaryRT(_sourceId, blitTargetDescriptor, FilterMode);
				_source = new RenderTargetIdentifier(_sourceId);
			}

			if (_isSourceAndDestinationSameTarget)
			{
				_destinationId = _temporaryRTId;
				cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode);
				_destination = new RenderTargetIdentifier(_destinationId);
			}
			else if (_settings.destinationType == BufferType.CameraColor)
			{
				_destinationId = -1;
				_destination = renderer.cameraColorTarget;
			}
			else
			{
				_destinationId = Shader.PropertyToID(_settings.destinationTextureId);
				cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode);
				_destination = new RenderTargetIdentifier(_destinationId);
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

			if (_isSourceAndDestinationSameTarget)
			{
				Blit(cmd, _source, _destination, _settings.blitMaterial, _settings.blitMaterialPassIndex);
				Blit(cmd, _destination, _source);
			}
			else
			{
				Blit(cmd, _source, _destination, _settings.blitMaterial, _settings.blitMaterialPassIndex);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}		
	}
}