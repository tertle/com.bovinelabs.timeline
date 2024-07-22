// <copyright file="ActionTimelineSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_REACTION
namespace BovineLabs.Timeline.Reaction
{
    using BovineLabs.Reaction.Data.Active;
    using BovineLabs.Reaction.Data.Core;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Reaction;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.IntegerTime;

    public partial struct ActionTimelineSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ActivatedJob
                {
                    TrackBindings = SystemAPI.GetComponentLookup<TrackBinding>(),
                    TargetsCustoms = SystemAPI.GetComponentLookup<TargetsCustom>(true),
                }
                .ScheduleParallel();

            new DeactivatedJob().ScheduleParallel();
        }

        [BurstCompile]
        [WithAll(typeof(Active))]
        [WithDisabled(typeof(ActivePrevious))]
        private partial struct ActivatedJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<TrackBinding> TrackBindings;

            [ReadOnly]
            public ComponentLookup<TargetsCustom> TargetsCustoms;

            private void Execute(
                Entity entity, ref Timer timer, EnabledRefRW<TimelineActive> timelineActive, in Targets targets, in ActionTimeline actionTimeline,
                in DynamicBuffer<DirectorBinding> directorBindings, in DynamicBuffer<ActionTimelineBinding> actionTimelineBindings)
            {
                timelineActive.ValueRW = true;
                timer.Time = new DiscreteTime(actionTimeline.InitialTime);

                foreach (var bindingTarget in actionTimelineBindings)
                {
                    var target = targets.Get(bindingTarget.Target, entity, this.TargetsCustoms);
                    if (target == Entity.Null)
                    {
                        continue;
                    }

                    foreach (var binding in directorBindings)
                    {
                        if (binding.TrackIdentifier != bindingTarget.TrackIdentifier)
                        {
                            continue;
                        }

                        ref var trackBinding = ref this.TrackBindings.GetRefRW(binding.TrackEntity).ValueRW;
                        trackBinding.Value = target;
                    }
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(ActivePrevious))]
        [WithDisabled(typeof(Active))]
        [WithAll(typeof(ActionTimeline))]
        private partial struct DeactivatedJob : IJobEntity
        {
            private static void Execute(EnabledRefRW<TimelineActive> timelineActive, in ActionTimeline actionTimeline, in Targets targets)
            {
                if (actionTimeline.Deactivate)
                {
                    timelineActive.ValueRW = false;
                }
            }
        }
    }
}
#endif
