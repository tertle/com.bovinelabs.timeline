// <copyright file="ClipLocalTimeSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.IntegerTime;

    [UpdateInGroup(typeof(TimelineUpdateSystemGroup))]
    public partial struct ClipLocalTimeSystem : ISystem
    {
        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new LocalTimeJob
                {
                    ExtrapolationLoopType = SystemAPI.GetComponentTypeHandle<ExtrapolationLoop>(true),
                    ExtrapolationPingPongType = SystemAPI.GetComponentTypeHandle<ExtrapolationPingPong>(true),
                    ExtrapolationHoldType = SystemAPI.GetComponentTypeHandle<ExtrapolationHold>(true),
                }
                .ScheduleParallel(state.Dependency);
        }

        [WithAll(typeof(TimelineActive))]
        [WithChangeFilter(typeof(TimerData))]
        [BurstCompile]
        private unsafe partial struct LocalTimeJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [ReadOnly] public ComponentTypeHandle<ExtrapolationLoop> ExtrapolationLoopType;
            [ReadOnly] public ComponentTypeHandle<ExtrapolationPingPong> ExtrapolationPingPongType;
            [ReadOnly] public ComponentTypeHandle<ExtrapolationHold> ExtrapolationHoldType;

            [NativeDisableUnsafePtrRestriction] private ExtrapolationLoop* loops;
            [NativeDisableUnsafePtrRestriction] private ExtrapolationPingPong* pingPongs;
            [NativeDisableUnsafePtrRestriction] private ExtrapolationHold* holds;

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.loops = chunk.GetComponentDataPtrRO(ref this.ExtrapolationLoopType);
                this.pingPongs = chunk.GetComponentDataPtrRO(ref this.ExtrapolationPingPongType);
                this.holds = chunk.GetComponentDataPtrRO(ref this.ExtrapolationHoldType);

                return true;
            }

            public void OnChunkEnd(
                in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            {
            }

            private void Execute(
                [EntityIndexInQuery] int entityIndexInQuery, ref LocalTime localTime, in TimerData timerData, in TimeTransform timeTransform)
            {
                UpdateLocalTime(ref localTime, timerData, timeTransform);

                if (this.loops != null)
                {
                    UpdateLoop(ref localTime, timerData, timeTransform, this.loops[entityIndexInQuery]);
                }

                if (this.pingPongs != null)
                {
                    UpdatePingPong(ref localTime, timerData, timeTransform, this.pingPongs[entityIndexInQuery]);
                }

                if (this.holds != null)
                {
                    UpdateHold(ref localTime, timerData, timeTransform, this.holds[entityIndexInQuery]);
                }
            }

            private static void UpdateLocalTime(ref LocalTime localTime, in TimerData timerData, in TimeTransform timeTransform)
            {
                localTime.Value = timeTransform.ToLocalTimeUnbound(timerData.Time);
            }

            private static void UpdateLoop(
                ref LocalTime localTime, in TimerData timerData, in TimeTransform timeTransform, in ExtrapolationLoop extrapolationLoop)
            {
                var duration = timeTransform.end - timeTransform.start;
                if (duration <= DiscreteTime.Zero)
                {
                    localTime.Value = DiscreteTime.Zero;
                }
                else if ((extrapolationLoop.ExtrapolateOptions & ExtrapolationPosition.Pre) != 0 && timerData.Time < timeTransform.start)
                {
                    var time = timerData.Time - timeTransform.start;
                    time = duration - (-time % duration);
                    localTime.Value = (time * timeTransform.scale) + timeTransform.clipIn;
                }
                else if ((extrapolationLoop.ExtrapolateOptions & ExtrapolationPosition.Post) != 0 && timerData.Time >= timeTransform.end)
                {
                    var time = (timerData.Time - timeTransform.start) % duration;
                    localTime.Value = (time * timeTransform.scale) + timeTransform.clipIn;
                }
            }

            private static void UpdatePingPong(
                ref LocalTime localTime, in TimerData timerData, in TimeTransform timeTransform, in ExtrapolationPingPong extrapolationPingPong)
            {
                var duration = timeTransform.end - timeTransform.start;
                if (duration <= DiscreteTime.Zero)
                {
                    localTime.Value = DiscreteTime.Zero;
                }
                else if ((extrapolationPingPong.ExtrapolateOptions & ExtrapolationPosition.Pre) != 0 && timerData.Time < timeTransform.start)
                {
                    var time = timerData.Time - timeTransform.start;
                    time = (duration * 2) - (-time % (duration * 2));
                    time = duration - (time - duration).Abs();
                    localTime.Value = (time * timeTransform.scale) + timeTransform.clipIn;
                }
                else if ((extrapolationPingPong.ExtrapolateOptions & ExtrapolationPosition.Post) != 0 && timerData.Time >= timeTransform.end)
                {
                    var time = timerData.Time - timeTransform.start;
                    time %= duration * 2;
                    time = duration - (time - duration).Abs();
                    localTime.Value = (time * timeTransform.scale) + timeTransform.clipIn;
                }
            }

            private static void UpdateHold(
                ref LocalTime localTime, in TimerData timerData, in TimeTransform timeTransform, in ExtrapolationHold extrapolationHold)
            {
                if ((extrapolationHold.ExtrapolateOptions & ExtrapolationPosition.Pre) != 0 && timerData.Time < timeTransform.start)
                {
                    localTime.Value = timeTransform.clipIn;
                }
                else if ((extrapolationHold.ExtrapolateOptions & ExtrapolationPosition.Post) != 0 && timerData.Time >= timeTransform.end)
                {
                    localTime.Value = ((timeTransform.end - timeTransform.start) * timeTransform.scale) + timeTransform.clipIn;
                }
            }
        }
    }
}
