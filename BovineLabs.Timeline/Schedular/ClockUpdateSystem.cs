﻿// <copyright file="ClockUpdateSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using BovineLabs.Core.Assertions;
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
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.Presentation)]
    [UpdateBefore(typeof(TimerUpdateSystem))]
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
                .WithAny<ClockTypeConstant, ClockTypeGameTime, ClockTypeUnscaledGameTime>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            state.Dependency = new ClockUpdateJob
                {
                    ClockDataType = SystemAPI.GetComponentTypeHandle<ClockData>(),

                    ClockConstantType = SystemAPI.GetComponentTypeHandle<ClockTypeConstant>(true),
                    ClockGameTimeType = SystemAPI.GetComponentTypeHandle<ClockTypeGameTime>(true),
                    ClockUnscaledGameTimeType = SystemAPI.GetComponentTypeHandle<ClockTypeUnscaledGameTime>(true),

                    GameTimeScale = UnityEngine.Time.timeScale,
                    GameTimeDeltaTime = new DiscreteTime(SystemAPI.Time.DeltaTime),
                    UnscaledGameTimeDeltaTime = new DiscreteTime(UnityEngine.Time.unscaledDeltaTime),
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

            public DiscreteTime GameTimeDeltaTime;
            public double GameTimeScale;
            public DiscreteTime UnscaledGameTimeDeltaTime;

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
