// <copyright file="DOTSTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Entities;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public abstract class DOTSTrack : TrackAsset
    {
        public virtual void BakeTrack(BakingContext context, ActiveRange range)
        {
            foreach (var clip in this.GetActiveClipsFromAllLayers())
            {
                if (!clip.InRangeInclLoops(range))
                {
                    continue;
                }

                var dotsClip = clip.asset as DOTSClip;
                if (dotsClip != null)
                {
                    var clipContext = context;
                    clipContext.Clip = clip;

                    var clipEntity = dotsClip.CreateClipEntity(clipContext);
                    if (clipEntity != Entity.Null)
                    {
                        context.SharedContextValues.ClipEntities.Add((clipEntity, clip));

                        dotsClip.Clip = clip;
                        dotsClip.Bake(clipEntity, clipContext);
                    }
                }
            }

            this.Bake(context);
            context.SharedContextValues.ClipEntities.Clear();
            context.SharedContextValues.CompositeTimers.Clear(); // TODO is this right?
        }

        public virtual void Bake(BakingContext context)
        {
            // if (context.Binding != null)
            // {
            //     trackEntity = context.CreateTrackEntity();
            // }
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
            if (director.GetGenericBinding(this) is IPropertyPreview preview)
            {
                preview.GatherProperties(director, driver);
            }
        }
    }
}
