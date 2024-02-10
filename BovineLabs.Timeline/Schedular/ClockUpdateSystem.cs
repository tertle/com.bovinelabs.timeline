// <copyright file="ClockUpdateSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using BovineLabs.Core.Assertions;
    using BovineLabs.Core.Time;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary>
    /// System that captures time update data from different clocks
    /// Copies from ClockTypeXXX Component types to ClockData
    /// ClockData is used by the timer system to update timers
    /// </summary>
    [UpdateInGroup(typeof(ScheduleSystemGroup))]
    public partial struct ClockUpdateSystem : ISystem
    {
        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAllRW<ClockData>()
                .WithAll<TimelineActive>() // We don't check per entity but this is just used to early out entire chunks
                .WithAny<ClockTypeConstant, ClockTypeGameTime, ClockTypeUnscaledGameTime, ClockTypeRealTime>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            var time = SystemAPI.GetSingleton<UnityTime>();

            state.Dependency = new ClockUpdateJob
                {
                    ClockDataType = SystemAPI.GetComponentTypeHandle<ClockData>(),

                    ClockConstantType = SystemAPI.GetComponentTypeHandle<ClockTypeConstant>(true),
                    ClockGameTimeType = SystemAPI.GetComponentTypeHandle<ClockTypeGameTime>(true),
                    ClockUnscaledGameTimeType = SystemAPI.GetComponentTypeHandle<ClockTypeUnscaledGameTime>(true),
                    ClockRealTimeType = SystemAPI.GetComponentTypeHandle<ClockTypeRealTime>(true),

                    GameTimeScale = time.TimeScale,
                    GameTimeDeltaTime = new DiscreteTime(SystemAPI.Time.DeltaTime),
                    UnscaledGameTimeDeltaTime = new DiscreteTime(time.UnscaledDeltaTime),
                    RealTimeDeltaTime = DiscreteTime.FromTicks(time.Ticks),
                }
                .ScheduleParallel(query, state.Dependency);
        }

        [BurstCompile]
        private unsafe struct ClockUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<ClockData> ClockDataType;

            [ReadOnly]
            public ComponentTypeHandle<ClockTypeConstant> ClockConstantType;

            [ReadOnly]
            public ComponentTypeHandle<ClockTypeGameTime> ClockGameTimeType;

            [ReadOnly]
            public ComponentTypeHandle<ClockTypeUnscaledGameTime> ClockUnscaledGameTimeType;

            [ReadOnly]
            public ComponentTypeHandle<ClockTypeRealTime> ClockRealTimeType;

            public DiscreteTime GameTimeDeltaTime;
            public double GameTimeScale;
            public DiscreteTime UnscaledGameTimeDeltaTime;
            public DiscreteTime RealTimeDeltaTime;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var clockData = (ClockData*)chunk.GetRequiredComponentDataPtrRW(ref this.ClockDataType);

                // TODO maybe due to infrequent updates this could be faster to check Active enable state?
                if (chunk.Has(ref this.ClockGameTimeType))
                {
                    var write = new ClockData
                    {
                        DeltaTime = this.GameTimeDeltaTime,
                        Scale = this.GameTimeScale,
                    };

                    UnsafeUtility.MemCpyReplicate(clockData, &write, sizeof(ClockData), chunk.Count);
                }
                else if (chunk.Has(ref this.ClockUnscaledGameTimeType))
                {
                    var write = new ClockData
                    {
                        DeltaTime = this.UnscaledGameTimeDeltaTime,
                        Scale = 1,
                    };

                    UnsafeUtility.MemCpyReplicate(clockData, &write, sizeof(ClockData), chunk.Count);
                }
                else if (chunk.Has(ref this.ClockRealTimeType))
                {
                    var write = new ClockData
                    {
                        DeltaTime = this.RealTimeDeltaTime,
                        Scale = 1,
                    };

                    UnsafeUtility.MemCpyReplicate(clockData, &write, sizeof(ClockData), chunk.Count);
                }
                else
                {
                    Check.Assume(sizeof(ClockTypeConstant) == sizeof(ClockData));

                    var clockConstantType = chunk.GetComponentDataPtrRO(ref this.ClockConstantType);
                    UnsafeUtility.MemCpy(clockData, clockConstantType, sizeof(ClockData) * chunk.Count);
                }
            }
        }
    }
}
