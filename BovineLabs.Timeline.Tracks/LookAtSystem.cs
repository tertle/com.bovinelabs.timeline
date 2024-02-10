// <copyright file="LookAtSystem.cs" company="BovineLabs">
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
    public partial struct LookAtSystem : ISystem
    {
        private NativeParallelHashMap<Entity, MixData<float3>> blendResults;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.blendResults = new NativeParallelHashMap<Entity, MixData<float3>>(64, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.blendResults.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var unblendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<LookAtAnimated>()
                .WithAll<TrackBinding, LocalTime, Active>()
                .WithNone<ClipWeight>()
                .Build();

            var blendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<LookAtAnimated>()
                .WithAll<TrackBinding, LocalTime, Active, ClipWeight>()
                .Build();

            var dependency1 = new Resize
                {
                    BlendData = this.blendResults,
                    UnblendedCount = unblendedQuery.CalculateEntityCountWithoutFiltering(),
                    BlendedCount = blendedQuery.CalculateEntityCountWithoutFiltering(),
                }
                .Schedule(state.Dependency);

            // TODO these could be parallel if we turned off safety
            var dependency2 = new UpdateLookAtTargetJob
                {
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                }
                .ScheduleParallel(state.Dependency);

            dependency2 = new UpdateLookForwardJob
                {
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                }
                .ScheduleParallel(dependency2);

            state.Dependency = JobHandle.CombineDependencies(dependency1, dependency2);
            state.Dependency = new AnimateUnblendedJob { BlendData = this.blendResults.AsParallelWriter() }.ScheduleParallel(unblendedQuery, state.Dependency);
            state.Dependency = new AccumulateWeightedAnimationJob { BlendData = this.blendResults }.Schedule(blendedQuery, state.Dependency);

            state.Dependency = new WriteAnimatedValuesJob
                {
                    BlendData = this.blendResults,
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(),
                }
                .ScheduleParallel(this.blendResults, 64, state.Dependency);
        }

        [BurstCompile]
        private struct Resize : IJob
        {
            public NativeParallelHashMap<Entity, MixData<float3>> BlendData;

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

            private void Execute(ref LookAtAnimated lookAtAnimated, in TrackBinding trackBinding, in LookAtTarget lookAtTarget)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bt))
                {
                    return;
                }

                if (!this.LocalTransforms.TryGetComponent(lookAtTarget.Target, out var lt))
                {
                    return;
                }

                // For the sake of normalization we use a normalized direction
                var dir = math.normalizesafe(lt.Position - bt.Position);
                lookAtAnimated.DefaultValue = lt.Position + dir;
            }
        }

        [WithAll(typeof(Active))]
        [WithNone(typeof(ActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(LookAtStartingDirection))]
        private partial struct UpdateLookForwardJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref LookAtAnimated lookAtAnimated, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                lookAtAnimated.DefaultValue = bindingTransform.Position + bindingTransform.Forward();
            }
        }

        [WithAll(typeof(Active))]
        [WithNone(typeof(ClipWeight))]
        [BurstCompile]
        private partial struct AnimateUnblendedJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<float3>>.ParallelWriter BlendData;

            private void Execute(ref LookAtAnimated animated, in TrackBinding binding, in LocalTime localTime)
            {
                JobHelpers.AnimateUnblendExecuteGeneric<float3, BlobCurveSampler3, LookAtAnimated>(binding, localTime, ref animated, this.BlendData);
            }
        }

        [WithAll(typeof(Active))]
        [BurstCompile]
        private partial struct AccumulateWeightedAnimationJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<float3>> BlendData;

            private void Execute(ref LookAtAnimated animated, in TrackBinding binding, in LocalTime localTime, in ClipWeight clip)
            {
                JobHelpers.AccumulateWeightedAnimationExecuteGeneric<float3, BlobCurveSampler3, LookAtAnimated>(binding, localTime, ref animated, clip, this.BlendData);
            }
        }

        [BurstCompile]
        private struct WriteAnimatedValuesJob : IJobParallelHashMapDefer
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

                var blend = JobHelpers.Blend<float3, Float3Mixer>(ref target, lt.ValueRO.Forward());
                lt.ValueRW.Rotation = quaternion.LookRotationSafe(blend - lt.ValueRO.Position, new float3(0, 1, 0));
            }
        }
    }
}
