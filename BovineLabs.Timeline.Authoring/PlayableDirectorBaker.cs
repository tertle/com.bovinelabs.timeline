// <copyright file="TimelineBaker.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core.Authoring.EntityCommands;
    using BovineLabs.Reaction.Data;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Entities;
    using Unity.IntegerTime;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public class PlayableDirectorBaker : Baker<PlayableDirector>
    {
        /// <inheritdoc/>
        public override void Bake(PlayableDirector director)
        {
            if (director.playableAsset is not TimelineAsset)
            {
                return;
            }

            var entity = this.CreateAdditionalEntity(TransformUsageFlags.ManualOverride);

            // TODO
            var builder = default(ActiveBuilder);
            // builder.WithActiveManual(true);
            var commands = new BakerCommands(this, entity);
            builder.ApplyTo(ref commands);

            this.AddComponent(entity, new Timer { Time = new DiscreteTime(director.initialTime), TimeScale = 1 });
            this.AddComponent<TimerPaused>(entity);
            this.SetComponentEnabled<TimerPaused>(entity, false);

            this.AddComponent<ClockData>(entity);

            switch (director.timeUpdateMode)
            {
                case DirectorUpdateMode.DSPClock:
                    this.AddComponent<ClockTypeRealTime>(entity);
                    Debug.LogWarning("DSP Clock mode not yet supported in DOTS. Using realtime clock instead");
                    break;

                case DirectorUpdateMode.GameTime:
                    this.AddComponent<ClockTypeGameTime>(entity);
                    break;

                case DirectorUpdateMode.UnscaledGameTime:
                    this.AddComponent<ClockTypeUnscaledGameTime>(entity);
                    break;

                case DirectorUpdateMode.Manual:
                    this.AddComponent(entity, new ClockTypeConstant
                    {
                        DeltaTime = DiscreteTime.Zero,
                        TimeScale = 1,
                    });
                    break;
            }

            var duration = new DiscreteTime(director.playableAsset.duration);
            switch (director.extrapolationMode)
            {
                case DirectorWrapMode.Hold:
                    // Auto Pause
                    this.AddComponent(entity, new TimerRange
                    {
                        Behaviour = RangeBehaviour.AutoPause,
                        Range = new DiscreteTimeInterval(DiscreteTime.Zero, duration),
                    });

                    break;
                case DirectorWrapMode.Loop:
                    this.AddComponent(entity, new TimerRange
                    {
                        Behaviour = RangeBehaviour.Loop,
                        Range = new DiscreteTimeInterval(DiscreteTime.Zero, duration),
                        LoopCount = 0,
                    });

                    break;
                case DirectorWrapMode.None:
                    // @TODO, make the sample last frame optional
                    this.AddComponent(entity, new TimerRange
                    {
                        Behaviour = RangeBehaviour.AutoStop,
                        Range = new DiscreteTimeInterval(DiscreteTime.Zero, duration),
                        SampleLastFrame = true,
                    });

                    break;
            }

            var context = new BakingContext(this, entity, this.GetEntity(TransformUsageFlags.None), director);

            ConvertPlayableDirector(context, ActiveRange.CompleteRange);

            var binders = this.AddBuffer<DirectorBinding>(entity);
            foreach (var b in context.SharedContextValues.BindingToClip)
            {
                binders.Add(new DirectorBinding
                {
                    TrackName = b.TrackName,
                    Binder = b.Binder,
                });
            }
        }

        public static void ConvertPlayableDirector(BakingContext context, ActiveRange range)
        {
            if (context.Director == null)
            {
                throw new ArgumentException("context.Director cannot be null");
            }

            var timeline = context.Director.playableAsset as TimelineAsset;
            if (timeline == null)
            {
                return;
            }

            ConvertTimeline(context, timeline, range);
        }

        private static void ConvertTimeline(BakingContext context, TimelineAsset timeline, ActiveRange range)
        {
            context.Baker.DependsOn(timeline);

            var entity = context.Timer;

            var cachedTimeDataEntities = context.SharedContextValues.TimeDataEntities.ToArray();
            context.SharedContextValues.TimeDataEntities.Clear();

            ConvertTracks(context, timeline.GetDOTSTracks(), range);

            var links = context.Baker.AddBuffer<TimerDataLink>(entity);
            foreach (var e in context.SharedContextValues.TimeDataEntities)
            {
                links.Add(new TimerDataLink { Value = e });
            }

            var timerLinks = context.Baker.AddBuffer<CompositeTimerLink>(context.Timer);
            foreach (var link in context.SharedContextValues.CompositeLinkEntities)
            {
                timerLinks.Add(new CompositeTimerLink { Value = link });
            }

            context.SharedContextValues.TimeDataEntities.Clear();
            context.SharedContextValues.TimeDataEntities.AddRange(cachedTimeDataEntities);
        }

        private static void ConvertTracks(BakingContext context, IEnumerable<DOTSTrack> dotsTracks, ActiveRange range)
        {
            foreach (var track in dotsTracks)
            {
                var trackContext = context;
                trackContext.Track = track;
                trackContext.Binding = default;

                var trackBinding = trackContext.Director!.GetGenericBinding(track); // as TrackBaseBinding;

                trackContext.Binding = context.GetBinding(track, trackBinding);

                ConvertTrack(trackContext, range);
            }
        }

        public static void ConvertTrack(BakingContext context, ActiveRange range)
        {
            var track = context.Track as DOTSTrack;
            if (track == null)
            {
                throw new ArgumentException("context.Track must be a valid DOTS track");
            }

            context.Baker.DependsOn(track);
            foreach (var clip in track.GetClips())
            {
                context.Baker.DependsOn(clip.asset);
            }

            track.BakeTrack(context, range);
        }


    }
}
