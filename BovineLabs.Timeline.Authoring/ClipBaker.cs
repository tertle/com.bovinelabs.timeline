// <copyright file="ClipBaker.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.Entities;
    using Unity.IntegerTime;
    using UnityEngine.Timeline;

    public static class ClipBaker
    {
        public static void AddClipBaseComponents(BakingContext context, Entity clipEntity, TimelineClip clip)
        {
            context.SharedContextValues.TimeDataEntities.Add(clipEntity);

            context.AddActive(clipEntity);

            context.Baker.AddComponent<TimerData>(clipEntity);
            context.Baker.AddComponent(clipEntity, clip.GetTimeTransform());
            context.Baker.AddComponent(clipEntity, clip.GetActiveRange());
            context.Baker.AddComponent(clipEntity, new LocalTime { Value = DiscreteTime.Zero });

            // var sourceAssetId = new SourceAssetInstanceId{ Value = clip.asset.GetInstanceID() };
            // baker.AddComponent(clipEntity, sourceAssetId);

            if ((clip.clipCaps & ClipCaps.Blending) != 0)
            {
                context.Baker.AddComponent(clipEntity, new ClipWeight { Value = 1 });
            }
        }

        public static void AddExtrapolationComponents(BakingContext context, Entity clipEntity, TimelineClip clip)
        {
            if (clip.hasPreExtrapolation || clip.hasPostExtrapolation)
            {
                // 'Continue' is default behaviour, so it has no component
                //      it is valid for one component to have multiple, one on pre, one on post
                var options = GetExtrapolationOptions(clip, TimelineClip.ClipExtrapolation.Hold);
                if (options != ExtrapolationPosition.None)
                {
                    context.Baker.AddComponent(clipEntity, new ExtrapolationHold
                    {
                        ExtrapolateOptions = options,
                    });
                }

                options = GetExtrapolationOptions(clip, TimelineClip.ClipExtrapolation.Loop);
                if (options != ExtrapolationPosition.None)
                {
                    context.Baker.AddComponent(clipEntity, new ExtrapolationLoop
                    {
                        ExtrapolateOptions = options,
                    });
                }

                options = GetExtrapolationOptions(clip, TimelineClip.ClipExtrapolation.PingPong);
                if (options != ExtrapolationPosition.None)
                {
                    context.Baker.AddComponent(clipEntity, new ExtrapolationPingPong
                    {
                        ExtrapolateOptions = options,
                    });
                }
            }
        }

        public static void AddMixCurvesComponents(BakingContext context, Entity clipEntity, TimelineClip clip)
        {
            var dotsClip = clip.asset as DOTSClip;
            if (dotsClip == null)
            {
                return;
            }

            var curve = clip.CreateClipWeightCurve();

            if (curve != null)
            {
                var blobCurve = BlobCurve.Create(curve);
                context.Baker.AddBlobAsset(ref blobCurve, out _);

                context.Baker.AddComponent(clipEntity, new AnimatedClipWeight
                {
                    Value = new BlobCurveSampler(blobCurve),
                });
            }
        }

        private static ExtrapolationPosition GetExtrapolationOptions(TimelineClip clip, TimelineClip.ClipExtrapolation mode)
        {
            var options = ExtrapolationPosition.None;
            if (clip.hasPreExtrapolation && clip.preExtrapolationMode == mode)
            {
                options |= ExtrapolationPosition.Pre;
            }

            if (clip.hasPostExtrapolation && clip.postExtrapolationMode == mode)
            {
                options |= ExtrapolationPosition.Post;
            }

            return options;
        }
    }
}
