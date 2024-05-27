// <copyright file="PositionTrackSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Core.Jobs;
    using BovineLabs.Timeline;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PositionTrackSystem : ISystem
    {
        private TrackBlendImpl<float3, BlobCurveSampler3, PositionAnimated> impl;

        /// <inheritdoc/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.impl.OnCreate(ref state);
        }

        /// <inheritdoc/>
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.impl.OnDestroy(ref state);
        }

        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var localTransforms = SystemAPI.GetComponentLookup<LocalTransform>();

            new ActivateResetJob { LocalTransforms = localTransforms }.ScheduleParallel();
            new DeactivateResetJob { LocalTransforms = localTransforms }.Schedule();

            new MoveToStartingPositionClipJob { LocalTransforms = localTransforms }.ScheduleParallel();

            var blendData = this.impl.Update(ref state);

            state.Dependency = new WritePositionJob { BlendData = blendData, LocalTransforms = localTransforms }
                .ScheduleParallel(blendData, 64, state.Dependency);
        }

        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct ActivateResetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionResetOnDeactivate positionResetOnDeactivate, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                positionResetOnDeactivate.Value = bindingTransform.Position;
            }
        }

        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct DeactivateResetJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(in PositionResetOnDeactivate positionResetOnDeactivate, in TrackBinding trackBinding)
            {
                var localTransform = this.LocalTransforms.GetRefRWOptional(trackBinding.Value);
                if (!localTransform.IsValid)
                {
                    return;
                }

                localTransform.ValueRW.Position = positionResetOnDeactivate.Value;
            }
        }

        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(MoveToStartingPosition))]
        private partial struct MoveToStartingPositionClipJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionAnimated positionAnimated, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                positionAnimated.DefaultValue = bindingTransform.Position;
            }
        }

        [BurstCompile]
        private struct WritePositionJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<float3>> BlendData;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransforms;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(this.BlendData, entryIndex, out var entity, out var target);

                var lt = this.LocalTransforms.GetRefRWOptional(entity);
                if (!lt.IsValid)
                {
                    return;
                }

                lt.ValueRW.Position = JobHelpers.Blend<float3, Float3Mixer>(ref target, lt.ValueRO.Position);
            }
        }
    }
}
