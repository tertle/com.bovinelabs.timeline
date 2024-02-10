// <copyright file="TimerUpdateSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using BovineLabs.Core.Extensions;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.IntegerTime;

    [UpdateAfter(typeof(ClockUpdateSystem))]
    [UpdateInGroup(typeof(ScheduleSystemGroup))]
    public partial struct TimerUpdateSystem : ISystem
    {
        private EntityQuery stoppedQuery;

        /// <inheritdoc/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.stoppedQuery = SystemAPI.QueryBuilder()
                .WithAll<TimelineActivePrevious, TimerDataLink>()
                .WithDisabled<TimelineActive>()
                .WithPresent<TimerPaused>()
                .Build();

            this.stoppedQuery.SetChangedVersionFilter(ComponentType.ReadWrite<TimelineActive>());
        }

        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new TimerStartedJob
                {
                    TimerDatas = SystemAPI.GetComponentLookup<TimerData>(),
                    Actives = SystemAPI.GetComponentLookup<TimelineActive>(),
                }
                .ScheduleParallel(state.Dependency);

            state.Dependency = new TimersUpdateJob
                {
                    TimerDatas = SystemAPI.GetComponentLookup<TimerData>(),
                    Actives = SystemAPI.GetComponentLookup<TimelineActive>(),
                    TimerPauseds = SystemAPI.GetComponentLookup<TimerPaused>(),
                    TimerDataLinks = SystemAPI.GetBufferLookup<TimerDataLink>(true),
                    CompositeTimerLinks = SystemAPI.GetBufferLookup<CompositeTimerLink>(true),
                    CompositeTimers = SystemAPI.GetComponentLookup<CompositeTimer>(true),
                    Timers = SystemAPI.GetComponentLookup<Timer>(),
                }
                .ScheduleParallel(state.Dependency);

            state.Dependency = new TimerStoppedJob
                {
                    TimerDataLinks = SystemAPI.GetBufferTypeHandle<TimerDataLink>(true),
                    TimerPausedHandle = SystemAPI.GetComponentTypeHandle<TimerPaused>(),
                    Actives = SystemAPI.GetComponentLookup<TimelineActive>(),
                }
                .ScheduleParallel(this.stoppedQuery, state.Dependency);
        }

        [WithAll(typeof(TimelineActive))]
        [WithDisabled(typeof(TimelineActivePrevious))]
        [WithChangeFilter(typeof(TimelineActive))]
        [BurstCompile]
        private partial struct TimerStartedJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimerData> TimerDatas;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimelineActive> Actives;

            private void Execute(ref Timer timer, in ClockData clockData, in DynamicBuffer<TimerDataLink> timerDataLinks)
            {
                timer.DeltaTime = DiscreteTime.Zero;
                timer.TimeScale = clockData.Scale;
                timer.Time = DiscreteTime.Zero;
                // TODO this doesn't seem to factor in initialize time

                foreach (var link in timerDataLinks.AsNativeArray())
                {
                    this.TimerDatas[link.Value] = new TimerData
                    {
                        DeltaTime = timer.DeltaTime,
                        TimeScale = timer.TimeScale,
                        Time = timer.Time,
                    };

                    this.Actives.SetComponentEnabled(link.Value, true);
                }
            }
        }

        [BurstCompile]
        private struct TimerStoppedJob : IJobChunk
        {
            [ReadOnly]
            public BufferTypeHandle<TimerDataLink> TimerDataLinks;

            public ComponentTypeHandle<TimerPaused> TimerPausedHandle;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimelineActive> Actives;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var timerPauseds = chunk.GetEnabledMask(ref this.TimerPausedHandle);
                var timerDataLinksAccessor = chunk.GetBufferAccessor(ref this.TimerDataLinks);

                var e = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (e.NextEntityIndex(out var index))
                {
                    timerPauseds[index] = false;

                    foreach (var link in timerDataLinksAccessor[index].AsNativeArray())
                    {
                        this.Actives.SetComponentEnabled(link.Value, false);
                    }
                }
            }
        }

        [WithAll(typeof(TimelineActive), typeof(TimelineActivePrevious))]
        [WithPresent(typeof(TimerPaused))]
        [WithNone(typeof(CompositeTimer))]
        [BurstCompile]
        private partial struct TimersUpdateJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimerData> TimerDatas;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimerPaused> TimerPauseds;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<TimelineActive> Actives;

            [ReadOnly]
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<TimerDataLink> TimerDataLinks;

            [ReadOnly]
            public BufferLookup<CompositeTimerLink> CompositeTimerLinks;

            [ReadOnly]
            public ComponentLookup<CompositeTimer> CompositeTimers;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            public ComponentLookup<Timer> Timers;

            private void Execute(
                Entity entity,
                ref Timer timer,
                ref TimerRange timerRange,
                in ClockData clockData,
                in DynamicBuffer<TimerDataLink> timerDataLinks)
            {
                var timerPaused = this.TimerPauseds.GetEnabledRefRW<TimerPaused>(entity);
                var active = this.Actives.GetEnableRefRWNoChangeFilter(entity);

                var previousTime = timer.Time;

                timer.DeltaTime = timerPaused.ValueRO ? DiscreteTime.Zero : clockData.DeltaTime;
                timer.Time += timer.DeltaTime;
                timer.TimeScale = clockData.Scale;

                if (!timerPaused.ValueRO)
                {
                    if (TimerRangeImpl.ApplyTimerRange(ref timer, ref timerRange, previousTime, timerPaused, active))
                    {
                        this.Actives.SetChangeFilter(entity);
                    }
                }

                var source = new TimerData
                {
                    DeltaTime = timer.DeltaTime,
                    TimeScale = timer.TimeScale,
                    Time = timer.Time,
                };

                this.Update(entity, source, timerDataLinks);
            }

            private void Update(
                Entity entity,
                in TimerData source,
                in DynamicBuffer<TimerDataLink> timerDataLinks)
            {
                foreach (var link in timerDataLinks.AsNativeArray())
                {
                    this.TimerDatas[link.Value] = source;
                }

                if (!this.CompositeTimerLinks.TryGetBuffer(entity, out var compositeLinks))
                {
                    return;
                }

                foreach (var compLink in compositeLinks.AsNativeArray())
                {
                    var composite = this.CompositeTimers[compLink.Value];
                    var newLinks = this.TimerDataLinks[compLink.Value];
                    ref var timer = ref this.Timers.GetRefRW(compLink.Value).ValueRW;

                    timer.Time = (source.Time * composite.Scale) + composite.Offset;
                    timer.DeltaTime = source.DeltaTime * composite.Scale;
                    timer.TimeScale = source.TimeScale * composite.Scale;

                    var active = source.Time >= composite.ActiveRange.Start && source.Time < composite.ActiveRange.End;
                    var activeRW = this.Actives.GetEnableRefRWNoChangeFilter(compLink.Value);
                    if (active != activeRW.ValueRO)
                    {
                        activeRW.ValueRW = active;
                        this.Actives.SetChangeFilter(compLink.Value);

                        // Enable or disable everything
                        if (active)
                        {
                            foreach (var link in newLinks.AsNativeArray())
                            {
                                this.Actives.SetComponentEnabled(link.Value, true);
                            }
                        }
                        else
                        {
                            foreach (var link in newLinks.AsNativeArray())
                            {
                                this.Actives.SetComponentEnabled(link.Value, false);
                            }
                        }
                    }

                    var newSource = new TimerData
                    {
                        DeltaTime = timer.DeltaTime,
                        TimeScale = timer.TimeScale,
                        Time = timer.Time,
                    };

                    this.Update(compLink.Value, newSource, newLinks);
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive), typeof(TimelineActivePrevious))]
        private partial struct TimerCompositeUpdateJob : IJobEntity
        {
            private void Execute()
            {
            }
        }
    }
}
