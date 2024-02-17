// <copyright file="EditorTimelineSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Editor
{
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using BovineLabs.Timeline.Schedular;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.IntegerTime;
    using UnityEditor.Timeline;
    using UnityEngine;

    [WorldSystemFilter(WorldSystemFilterFlags.Editor)]
    [UpdateBefore(typeof(TimerUpdateSystem))]
    [UpdateInGroup(typeof(ScheduleSystemGroup))]
    public partial class EditorTimelineSystem : SystemBase
    {
        private NativeHashSet<Entity> toDisable;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            this.toDisable = new NativeHashSet<Entity>(1, Allocator.Persistent);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            this.toDisable.Dispose();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            var isActiveQuery = SystemAPI.QueryBuilder().WithAll<Timer, ClockData, TimelineActive>().Build();

            this.DisableUnselected();
            this.ResetActive(isActiveQuery);
            this.EnableSelected(isActiveQuery);
        }

        private static TimelineWindow[] GetAllOpenEditorWindows()
        {
            return Resources.FindObjectsOfTypeAll<TimelineWindow>();
        }

        private void DisableUnselected()
        {
            using var it = this.toDisable.GetEnumerator();

            while (it.MoveNext())
            {
                this.EntityManager.SetComponentEnabled<TimelineActive>(it.Current, false);
            }

            this.toDisable.Clear();
        }

        private void ResetActive(EntityQuery isActiveQuery)
        {
            // Reset everything in case of deselection so that it'll return to state at 0
            // If something is selected the time here will just be overridden next
            foreach (var e in isActiveQuery.ToEntityArray(this.WorldUpdateAllocator))
            {
                this.toDisable.Add(e);
                this.EntityManager.SetComponentData(e, new Timer { Time = new DiscreteTime(0), TimeScale = 1 });
            }
        }

        private void EnableSelected(EntityQuery isActiveQuery)
        {
            var entities = new NativeList<Entity>(this.WorldUpdateAllocator);
            var mask = isActiveQuery.GetEntityQueryMask();

            foreach (var w in GetAllOpenEditorWindows())
            {
                if (w.state?.masterSequence?.director == null)
                {
                    continue;
                }

                var director = w.state.masterSequence.director;

                this.EntityManager.Debug.GetEntitiesForAuthoringObject(director, entities);

                foreach (var e in entities)
                {
                    if (mask.MatchesIgnoreFilter(e))
                    {
                        this.EntityManager.SetComponentEnabled<TimelineActive>(e, true);
                        this.EntityManager.SetComponentData(e, new Timer { Time = new DiscreteTime(director.time), TimeScale = 1 });
                        this.toDisable.Remove(e);
                        break;
                    }
                }
            }
        }
    }
}
