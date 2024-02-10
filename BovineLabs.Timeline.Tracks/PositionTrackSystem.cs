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
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PositionTrackSystem : ISystem
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
            // TODO these could be parallel if we turned off safety
            var dependency1 = new MoveToStartingPositionJob
                {
                    LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                }
                .ScheduleParallel(state.Dependency);

            var unblendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<PositionAnimated>()
                .WithAll<TrackBinding, LocalTime, TimelineActive>()
                .WithNone<ClipWeight>()
                .Build();

            var blendedQuery = SystemAPI.QueryBuilder()
                .WithAllRW<PositionAnimated>()
                .WithAll<TrackBinding, LocalTime, TimelineActive, ClipWeight>()
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

        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(MoveToStartingPosition))]
        private partial struct MoveToStartingPositionJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionAnimated rotationAnimated, in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    return;
                }

                rotationAnimated.DefaultValue = bindingTransform.Position;
            }
        }

        [BurstCompile]
        private struct ResizeJob : IJob
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

        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(ClipWeight))]
        [BurstCompile]
        private partial struct AnimateUnblendedJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<float3>>.ParallelWriter BlendData;

            private void Execute(ref PositionAnimated animated, in TrackBinding binding, in LocalTime localTime)
            {
                JobHelpers.AnimateUnblendExecuteGeneric<float3, BlobCurveSampler3, PositionAnimated>(binding, localTime, ref animated, this.BlendData);
            }
        }

        [WithAll(typeof(TimelineActive))]
        [BurstCompile]
        private partial struct AccumulateWeightedAnimationJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, MixData<float3>> BlendData;

            private void Execute(ref PositionAnimated animated, in TrackBinding binding, in LocalTime localTime, in ClipWeight clip)
            {
                JobHelpers.AccumulateWeightedAnimationExecuteGeneric<float3, BlobCurveSampler3, PositionAnimated>(binding, localTime, ref animated, clip, this.BlendData);
            }
        }

        [BurstCompile]
        private struct WriteRotationJob : IJobParallelHashMapDefer
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
