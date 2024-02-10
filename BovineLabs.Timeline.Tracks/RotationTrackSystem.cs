// <copyright file="RotationSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Core.Jobs;
    using BovineLabs.Reaction.Data;
    using BovineLabs.Timeline;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct RotationTrackSystem : ISystem
    {
        private NativeParallelHashMap<Entity, MixData<quaternion>> blendResults;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.blendResults = new NativeParallelHashMap<Entity, MixData<quaternion>>(64, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.blendResults.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // TODO these could be parallel if we turned off safety
            var dependency1 = new UpdateLookAtTargetJob
                {
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                }
                .ScheduleParallel(state.Dependency);

            dependency1 = new LookAtStartingDirectionJob
                {
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                }
                .ScheduleParallel(dependency1);

            var unblendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<RotationAnimated>()
                .WithAll<TrackBinding, LocalTime, Active>()
                .WithNone<ClipWeight>()
                .Build();

            var blendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<RotationAnimated>()
                .WithAll<TrackBinding, LocalTime, Active, ClipWeight>()
                .Build();

            var dependency2 = new ResizeJob
                {
                    BlendData = this.blendResults,
                    UnblendedCount = unblendedQuery.CalculateEntityCountWithoutFiltering(),
                    BlendedCount = blendedQuery.CalculateEntityCountWithoutFiltering(),
                }
                .Schedule(state.Dependency);

            state.Dependency = JobHandle.CombineDependencies(dependency1, dependency2);
            state.Dependency = new AnimateUnblendedJob { BlendData = this.blendResults.AsParallelWriter() }.ScheduleParallel(unblendedQuery, state.Dependency);
            state.Dependency = new AccumulateWeightedAnimationJob { BlendData = this.blendResults }.Schedule(blendedQuery, state.Dependency);

            state.Dependency = new WriteRotationJob
                {
                    BlendData = this.blendResults,
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(),
                }
                .ScheduleParallel(this.blendResults, 64, state.Dependency);
        }

        [BurstCompile]
        private struct ResizeJob : IJob
        {
            public NativeParallelHashMap<Entity, MixData<quaternion>> BlendData;

            public int UnblendedCount;
            public int BlendedCount;

            public void Execute()
            {
                this.BlendData.Clear();
                if (this.BlendData.Capacity < this.UnblendedCount + this.BlendedCount)
                {
                    this.BlendData.Capacity = this.UnblendedCount + this.BlendedCount;
                }
            }
        }

        [WithAll(typeof(Active))]
        private partial struct UpdateLookAtTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref RotationAnimated rotationAnimated, in TrackBinding trackBinding, in LookAtTarget lookAtTarget)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bt))
                {
                    return;
                }

                if (!this.LocalTransforms.TryGetComponent(lookAtTarget.Target, out var lt))
                {
                    return;
                }

                rotationAnimated.DefaultValue = quaternion.LookRotation(lt.Position - bt.Position, math.up());
            }
        }

        [WithAll(typeof(Active))]
        [WithNone(typeof(ActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(LookAtStartingDirection))]
        private partial struct LookAtStartingDirectionJob : IJobEntity
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

        [WithAll(typeof(Active))]
        [WithNone(typeof(ClipWeight))]
        [BurstCompile]
        private partial struct AnimateUnblendedJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<quaternion>>.ParallelWriter BlendData;

            private void Execute(ref RotationAnimated animated, in TrackBinding binding, in LocalTime localTime)
            {
                JobHelpers.AnimateUnblendExecuteGeneric<quaternion, BlobCurveSampler4<quaternion>, RotationAnimated>(binding, localTime, ref animated, this.BlendData);
            }
        }

        [WithAll(typeof(Active))]
        [BurstCompile]
        private partial struct AccumulateWeightedAnimationJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<quaternion>> BlendData;

            private void Execute(ref RotationAnimated animated, in TrackBinding binding, in LocalTime localTime, in ClipWeight clip)
            {
                JobHelpers.AccumulateWeightedAnimationExecuteGeneric<quaternion, BlobCurveSampler4<quaternion>, RotationAnimated>(binding, localTime, ref animated, clip, this.BlendData);
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
