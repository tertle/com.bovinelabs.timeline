// <copyright file="SubDirectorClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring.Nesting
{
    using System;
    using System.Linq;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    [Serializable]
    public class SubDirectorClip : DOTSClip, ITimelineClipAsset, IPropertyPreview
    {
        /// <summary>The sub timeline as a playable director</summary>
        public ExposedReference<PlayableDirector> SubDirector;

        // /// <summary>Optional reference to a 'take' in the nested timeline</summary>
        // public EditTrack Take;

        /// <summary>The default duration of the timeline. This is set in the editor based on the length of the timeline assigned</summary>
        [HideInInspector]
        public double DefaultClipDuration = TimelineClip.kDefaultClipDurationInSeconds;

        /// <inheritdoc/>
        /// <remarks>Converts the sub director timeline</remarks>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            // instead of returning a single clip entity, compiles the nested timeline.
            var player = this.SubDirector.Resolve(context.Director);
            if (player != null)
            {
                var composites = context.SharedContextValues.CompositeLinkEntities.ToArray();
                context.SharedContextValues.CompositeLinkEntities.Clear();

                context = context.CreateCompositeTimer();
                context.Director = player;

                // if (Take != null)
                // Take.ConvertEdits(context, context.Clip.GetSubTimelineRange());
                // else
                PlayableDirectorBaker.ConvertPlayableDirector(context, context.Clip!.GetSubTimelineRange());

                context.SharedContextValues.CompositeLinkEntities.Clear();
                context.SharedContextValues.CompositeLinkEntities.AddRange(composites);
                context.SharedContextValues.CompositeLinkEntities.Add(context.Timer);
            }
        }

        /// <summary> Propagate any gather calls on to the Sub-Timeline. </summary>
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director == null)
            {
                return;
            }

            var subDir = this.SubDirector.Resolve(director);
            if (subDir != null)
            {
                var tlAsset = subDir.playableAsset as TimelineAsset;
                if (tlAsset != null)
                {
                    tlAsset.GatherProperties(subDir, driver);
                }
            }
        }


        /// <inheritdoc/>
        public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier; /*| ClipCaps.Looping;*/

        /// <summary>This is the default duration of the clip used by the UI</summary>
        public override double duration => /*Take != null ? Take.RemappedDuration : */DefaultClipDuration;

        // /// <summary>Gather nested playable directors</summary>
        // public void GetNestedDirectors(IExposedPropertyTable propertyTable, List<PlayableDirector> nestedDirectors)
        // {
        //     var director = propertyTable.GetReferenceValue(SubDirector.exposedName, out bool isValid) as PlayableDirector;
        //     if (isValid && director != null)
        //         nestedDirectors.Add(director);
        // }

        /// <summary>Overrides the playable creation</summary>
        /// <remarks>Needed to make the sub director in the window contain the correct time</remarks>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var director = go.GetComponent<PlayableDirector>();
            if (director != null)
            {
                director = director.GetReferenceValue(this.SubDirector.exposedName, out _) as PlayableDirector;
            }

            var timeSync = ScriptPlayable<TimeSyncBehaviour>.Create(graph);
            timeSync.GetBehaviour().Director = director;

            return timeSync;
        }

        // Behaviour used to set the time on the nested playable behaviour. This is required to have the nested timeline director
        // report the correct time
        private class TimeSyncBehaviour : PlayableBehaviour
        {
            public PlayableDirector? Director;

            public override void PrepareFrame(Playable playable, FrameData info)
            {
                if (this.Director != null)
                {
                    this.Director.time = playable.GetTime();
                }
            }
        }
    }
}
