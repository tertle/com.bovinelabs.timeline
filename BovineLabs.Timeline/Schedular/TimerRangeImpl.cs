// <copyright file="TimerRangeImpl.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using BovineLabs.Reaction.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Entities;
    using Unity.IntegerTime;

    public static class TimerRangeImpl
    {
        public static bool ApplyTimerRange(
            ref Timer timer, ref TimerRange range, DiscreteTime previousTime, EnabledRefRW<TimerPaused> timerPauseds, EnabledRefRW<Active> actives)
        {
            switch (range.Behaviour)
            {
                case RangeBehaviour.AutoStop:
                    if (ApplyAutoStop(ref timer, ref range, previousTime, actives))
                    {
                        return true;
                    }

                    break;
                case RangeBehaviour.AutoPause:
                    ApplyAutoPause(ref timer, ref range, timerPauseds);

                    break;
                case RangeBehaviour.Loop:
                    ApplyLoop(ref timer, ref range);

                    break;
            }

            return false;
        }

        private static bool ApplyAutoStop(ref Timer timer, ref TimerRange range, DiscreteTime previousTime, EnabledRefRW<Active> actives)
        {
            timer.Time = timer.Time.Max(range.Range.Start);
            if (timer.Time >= range.Range.End)
            {
                if (range.SampleLastFrame && previousTime < range.Range.End)
                {
                    timer.Time = range.Range.End;
                }
                else
                {
                    timer.Time = range.Range.Start;
                    actives.ValueRW = false;

                    return true;
                    // state.TimerStateFlags |= TimerStateFlags.Completed;
                }
            }

            return false;
        }

        private static void ApplyAutoPause(ref Timer timer, ref TimerRange clamp, EnabledRefRW<TimerPaused> timerPauseds)
        {
            timer.Time = clamp.Range.Clamp(timer.Time);
            if (timer.Time == clamp.Range.End)
            {
                timerPauseds.ValueRW = true;
            }
        }

        private static void ApplyLoop(ref Timer timer, ref TimerRange range)
        {
            if (timer.Time < range.Range.Start)
            {
                timer.Time = range.Range.Start;
            }
            else if (timer.Time >= range.Range.End)
            {
                if (range.Range.Start == range.Range.End)
                {
                    timer.Time = range.Range.Start;
                }
                else
                {
                    var deltaTicks = range.Range.Duration.Value;
                    var timeTicks = timer.Time.Value - range.Range.Start.Value;
                    // var loops = (uint)(timeTicks / deltaTicks);
                    // state.TimerStateFlags |= TimerStateFlags.Looped;
                    range.LoopCount += (uint)(timeTicks / deltaTicks);
                    timer.Time = DiscreteTime.FromTicks(range.Range.Start.Value + (timeTicks % deltaTicks));
                }
            }
        }
    }
}
