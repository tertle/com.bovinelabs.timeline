// <copyright file="ClipWeightSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(TimelineSystemGroup))]
    [UpdateAfter(typeof(ClipLocalTimeSystem))]
    public partial struct ClipWeightSystem : ISystem
    {
        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AnimatedClipWeightJob().ScheduleParallel();
        }

        [BurstCompile]
        private partial struct AnimatedClipWeightJob : IJobEntity
        {
            private static void Execute(ref ClipWeight clipWeight, ref AnimatedClipWeight clipBlob, in LocalTime localTime)
            {
                clipWeight.Value = 1;
                if (clipBlob.Value.IsCreated)
                {
                    clipWeight.Value = clipBlob.Value.Evaluate((float)localTime.Value);
                }
            }
        }
    }
}
