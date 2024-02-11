﻿// <copyright file="RotationTrackSystem.cs" company="BovineLabs">
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
    public partial struct RotationTrackSystem : ISystem
    {
        private TrackBlendImpl<quaternion, BlobCurveSampler4<quaternion>, RotationAnimated> impl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new LookAtTargetClipJob { LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true) }
                .ScheduleParallel(state.Dependency);

            state.Dependency = new LookAtStartingDirectionClipJob { LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true) }
                .ScheduleParallel(state.Dependency);

            var blendData = this.impl.Update(ref state);

            state.Dependency = new WriteRotationJob
                {
                    BlendData = blendData,
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(),
                }
                .ScheduleParallel(blendData, 64, state.Dependency);
        }

        [WithAll(typeof(TimelineActive))]
        private partial struct LookAtTargetClipJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref RotationAnimated rotationAnimated, in TrackBinding trackBinding, in LookAtTarget lookAtTarget)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bt) ||
                    !this.LocalTransforms.TryGetComponent(lookAtTarget.Target, out var lt))
                {
                    return;
                }

                rotationAnimated.DefaultValue = quaternion.LookRotation(lt.Position - bt.Position, math.up());
            }
        }

        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(LookAtStartingDirection))]
        private partial struct LookAtStartingDirectionClipJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref RotationAnimated rotationAnimated, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                rotationAnimated.DefaultValue = bindingTransform.Rotation;
            }
        }

        [BurstCompile]
        private struct WriteRotationJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<quaternion>> BlendData;

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

                lt.ValueRW.Rotation = JobHelpers.Blend<quaternion, QuaternionMixer>(ref target, lt.ValueRO.Rotation);
            }
        }
    }
}
