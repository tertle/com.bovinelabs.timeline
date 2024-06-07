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

            // TODO all these could be run in parallel
            new ActivateResetJob { LocalTransforms = localTransforms }.ScheduleParallel();
            new DeactivateResetJob { LocalTransforms = localTransforms }.Schedule();
            new PositionOffsetJob { LocalTransforms = localTransforms }.ScheduleParallel();
            new PositionTargetJob { LocalTransforms = localTransforms }.ScheduleParallel();
            new MoveToStartingPositionClipJob { LocalTransforms = localTransforms }.ScheduleParallel();

            var blendData = this.impl.Update(ref state);

            state.Dependency = new WritePositionJob { BlendData = blendData, LocalTransforms = localTransforms }
                .ScheduleParallel(blendData, 64, state.Dependency);
        }

        [BurstCompile]
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

        [BurstCompile]
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

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        private partial struct PositionOffsetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionAnimated positionAnimated, in PositionOffset positionOffset, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                var offset = positionOffset.Type switch
                {
                    OffsetType.World => positionOffset.Offset,
                    OffsetType.Local => bindingTransform.TransformPoint(positionOffset.Offset),
                    _ => float3.zero,
                };

                positionAnimated.DefaultValue = bindingTransform.Position + offset;
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        private partial struct PositionTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionAnimated positionAnimated, in PositionTarget positionTarget)
            {
                if (!this.LocalTransforms.TryGetComponent(positionTarget.Target, out var targetTransform))
                {
                    return;
                }

                var offset = positionTarget.Type switch
                {
                    OffsetType.World => positionTarget.Offset,
                    OffsetType.Local => targetTransform.TransformPoint(positionTarget.Offset),
                    _ => float3.zero,
                };

                positionAnimated.DefaultValue = targetTransform.Position + offset;
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(PositionMoveToStart))]
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
            public NativeParallelHashMap<Entity, MixData<float3>>.ReadOnly BlendData;

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