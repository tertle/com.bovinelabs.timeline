// <copyright file="SubTimelineClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using Unity.Entities;
    using UnityEngine.Timeline;

    /// <summary>
    /// SubTimelineClip - is a clip that can build DOTS compatible tracks from a timeline asset.
    ///   The clip itself stores the tags
    /// </summary>
    [Serializable]
    public class SubTimelineClip : DOTSClip, ITimelineClipAsset
    {
        public TimelineAsset? Timeline;
        public TrackKeyBindings TrackBindings;

        public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

        /// <inheritdoc/>
        public override double duration => this.Timeline != null ? this.Timeline.duration : base.duration;

        /// <inheritdoc/>
        /// <remarks>Converts the sub director timeline. </remarks>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            if (this.Timeline != null)
            {
                var composites = context.SharedContextValues.CompositeLinkEntities.ToArray();
                context.SharedContextValues.CompositeLinkEntities.Clear();

                context.Baker.DependsOn(this.Timeline);

                var range = context.Clip!.GetSubTimelineRange();
                var newContext = context.CreateCompositeTimer();
                newContext.Director = null;

                foreach (var track in this.Timeline.GetDOTSTracks())
                {
                    newContext.Track = track;
                    newContext.Clip = null;
                    newContext.Binding = context.GetBinding(track, this.TrackBindings.FindObject(track));

                    PlayableDirectorBaker.ConvertTrack(newContext, range);
                }

                context.SharedContextValues.CompositeLinkEntities.Clear();
                context.SharedContextValues.CompositeLinkEntities.AddRange(composites);
            }
        }

        private void OnValidate()
        {
            if (this.Timeline != null)
            {
                this.TrackBindings.SyncToTimeline(this.Timeline);
            }
        }

        // public void GetTags(ICollection<Hash128> tags)
        // {
        //     foreach (var pairs in TrackBindings.Bindings)
        //     {
        //         if (pairs.Tag != null)
        //             tags.Add(pairs.Tag.Tag);
        //     }
        // }
    }
}
