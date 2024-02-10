// <copyright file="ConversionContext.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core.Authoring.EntityCommands;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Entities;
    using Unity.IntegerTime;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;
    using Object = UnityEngine.Object;

    /// <summary> Relevant information about the current timeline conversion. </summary>
    public struct BakingContext
    {
        /// <summary>The Conversion System</summary>
        public IBaker Baker;

        /// <summary>The current timer entity for this conversion</summary>
        public Entity Timer;

        /// <summary>The target object being converted. This is the top most gameObject with a PlayableDirector component.</summary>
        public Entity Target;

        /// <summary>The current playable director being converted. For the top most director, this is the PlayableDirector.</summary>
        public PlayableDirector? Director;

        /// <summary>The current track being converted</summary>
        public TrackAsset? Track;

        /// <summary>The current clip being converted</summary>
        public TimelineClip? Clip;

        /// <summary>The current identified for the binding</summary>
        public Binding? Binding;

        /// <summary>Values that should be maintained across context copies</summary>
        public SharedContextValues SharedContextValues;

        public BakingContext(IBaker baker, Entity timer, Entity target, PlayableDirector director)
        {
            this.Baker = baker;
            this.Timer = timer;
            this.Target = target;
            this.Director = director;
            this.Binding = null;
            this.Clip = null;
            this.Track = null;

            this.SharedContextValues = new SharedContextValues();
        }
    }

    public class Binding
    {
        public readonly string Name;
        public readonly Entity Target;

        public Binding(string name, Entity target)
        {
            this.Name = name;
            this.Target = target;
        }
    }

    /// <summary> Managed object to track values that should be maintained across conversion context copies. </summary>
    public class SharedContextValues
    {
        /// <summary>The current track priorities</summary>
        public int TrackPriority;

        /// <summary>The current list of clip entities to compile</summary>
        public List<(Entity ClipEntity, TimelineClip Clip)> ClipEntities = new();
        public readonly Dictionary<Entity, CompositeTimer> CompositeTimers = new();
        public readonly List<Entity> TimeDataEntities = new();
        public readonly List<Entity> CompositeLinkEntities = new();
        public readonly List<(string TrackName, Entity Binder)> BindingToClip = new();
    }

    /// <summary>
    /// Extension methods for ConversionContext
    /// </summary>
    public static class ConversionContextExtensions
    {
        /// <summary>
        /// Create an Entity, binding it to the target conversion object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static Entity CreateEntity(this BakingContext context, string? name = null)
        {
            return context.Baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name);
        }

        /// <summary>
        /// Create a composite timer entity using the context TimelineClip values. This requires the context to contain an existing Timer (which can be composite)
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A new entity containing a timer</returns>
        /// <exception cref="System.ArgumentException">Thrown if no TimelineClip is supplied</exception>
        public static BakingContext CreateCompositeTimer(this BakingContext context)
        {
            if (context.Clip == null)
            {
                throw new ArgumentException("CreateCompositeTimer requires a TimelineClip to create a CompositeTimer");
            }

            return CreateCompositeTimer(
                context,
                new ActiveRange
                {
                    Start = new DiscreteTime(context.Clip.extrapolatedStart),
                    End = new DiscreteTime(context.Clip.extrapolatedStart) + new DiscreteTime(context.Clip.extrapolatedDuration),
                },
                new DiscreteTime(context.Clip.clipIn) + (new DiscreteTime(-context.Clip.start) * context.Clip.timeScale),
                context.Clip.timeScale,
                context.Clip.displayName + " (Composite Timer)");
        }

        /// <summary>
        /// Create a composite timer using preset values
        /// </summary>
        /// <param name="context">The current ConversionContext</param>
        /// <param name="range">The range, relative to the parent timer, that this timer is active.</param>
        /// <param name="offset">The time offset of this timer relative to the parent timer.</param>
        /// <param name="scale">The scale offset of this timer relative to the parent timer.</param>
        /// <param name="name">The name of the entity.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the conversion context does not contain the required values for creating a composite timer</exception>
        public static BakingContext CreateCompositeTimer(this BakingContext context, ActiveRange range, DiscreteTime offset, double scale, string name)
        {
            if (context.Timer == default)
            {
                throw new ArgumentException("ConversionContext is invalid for creating a CompositeTimer");
            }

            var entity = context.CreateEntity(name);

            var parentScale = 1.0;
            var parentOffset = DiscreteTime.Zero;
            var masterTimer = context.Timer;

            if (context.SharedContextValues.CompositeTimers.TryGetValue(context.Timer, out var parent))
            {
                parentScale = parent.Scale;
                parentOffset = parent.Offset;
                masterTimer = parent.SourceTimer;
            }

            context.AddActive(entity);

            context.Baker.AddComponent(entity, new Timer { TimeScale = 1 });

            var composite = new CompositeTimer
            {
                SourceTimer = masterTimer,
                Offset = offset + (parentOffset * scale),
                Scale = scale * parentScale,
                ActiveRange = new ActiveRange
                {
                    Start = (range.Start / parentScale) - parentOffset,
                    End = (range.End / parentScale) - parentOffset,
                },
            };

            context.Baker.AddComponent(entity, composite);

            var newContext = context;
            newContext.Timer = entity;

            context.SharedContextValues.CompositeTimers.Add(context.Timer, composite);

            return newContext;
        }

        /// <summary>
        /// Creates an entity for the track. This is used to provide an active range for any prefabs that are spawned on the tracks.
        /// Creates a tag, that is active while the corresponding timer is active
        /// </summary>
        public static Entity CreateTrackEntity(this BakingContext context)
        {
            if (context.Track == null || context.Timer == Entity.Null || context.Binding == null)
            {
                throw new ArgumentException("Track Entities require a track, a timer and a tag");
            }

            var linked = CreateEntity(context, context.Track.name);
            context.AddActive(linked);

            context.Baker.AddComponent(linked, new TrackBinding { Value = context.Binding.Target });
            context.SharedContextValues.BindingToClip.Add((context.Binding.Name, linked));
            // context.Baker.AddComponent(linked, new ActiveRange
            // {
            //     Start = DiscreteTime.MinValue,
            //     End = DiscreteTime.MaxValue,
            // });

            context.Baker.AddComponent<TimerData>(linked);
            context.SharedContextValues.TimeDataEntities.Add(linked);

            return linked;
        }

        /// <summary> Create an entity representing a timeline clip. </summary>
        public static Entity CreateClipEntity(this BakingContext context)
        {
            if (context.Clip == null)
            {
                throw new ArgumentException("context.Clip cannot be null");
            }

            string name = $"{context.Clip.displayName} (ClipEntity)";

            var entity = CreateEntity(context, name);
            ClipBaker.AddClipBaseComponents(context, entity,  context.Clip);
            ClipBaker.AddExtrapolationComponents(context, entity, context.Clip);
            ClipBaker.AddMixCurvesComponents(context, entity, context.Clip);

            if (context.Binding != null)
            {
                context.Baker.AddComponent(entity, new TrackBinding { Value = context.Binding.Target });
                context.SharedContextValues.BindingToClip.Add((context.Binding.Name, entity));
            }

            return entity;
        }

        public static Binding GetBinding(this BakingContext context, DOTSTrack track, Object? trackBinding)
        {
            Entity entity = Entity.Null;

            if (trackBinding != null)
            {
                entity = trackBinding switch
                {
                    GameObject go => context.Baker.GetEntity(go, TransformUsageFlags.None),
                    Component component => context.Baker.GetEntity(component, TransformUsageFlags.None),
                    _ => Entity.Null,
                };
            }

            return new Binding(track.name, entity);
        }

        public static void AddActive(this BakingContext context, Entity entity)
        {
            context.Baker.AddComponent<TimelineActive>(entity);
            context.Baker.SetComponentEnabled<TimelineActive>(entity, false);

            context.Baker.AddComponent<TimelineActivePrevious>(entity);
            context.Baker.SetComponentEnabled<TimelineActivePrevious>(entity, false);
        }
    }
}
