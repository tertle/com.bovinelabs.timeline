// <copyright file="TimelineClipExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.IntegerTime;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Timeline;
    using Hash128 = Unity.Entities.Hash128;

    public static class TimelineClipExtensions
    {
        /// <summary>
        /// Returns the start and end range for a TimelineClip as an ActiveRange
        /// </summary>
        public static ActiveRange GetActiveRange(this TimelineClip clip)
        {
            var activeRange = new ActiveRange
            {
                Start = new DiscreteTime(clip.extrapolatedStart),
                End = new DiscreteTime(clip.extrapolatedStart) + new DiscreteTime(clip.extrapolatedDuration),
            };

            return activeRange;
        }

        /// <summary>
        /// Gets the parent to local time transform for a TimelineClip
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static TimeTransform GetTimeTransform(this TimelineClip clip)
        {
            return new TimeTransform
            {
                start = new DiscreteTime(clip.start),
                end = new DiscreteTime(clip.end),
                scale = clip.timeScale,
                clipIn = new DiscreteTime(clip.clipIn),
            };
        }

        public static Hash128 GetMixCurveHash(this TimelineClip clip)
        {
            if (clip == null)
            {
                return default;
            }

            if (clip.mixInDuration < float.Epsilon && clip.mixOutDuration < float.Epsilon)
            {
                return default;
            }

            var A = clip.mixInDuration.GetHashCode();
            var B = clip.mixInDuration > 0 ? clip.mixInCurve.GetHashCode() : 0;
            var C = clip.mixOutDuration.GetHashCode();
            var D = clip.mixOutDuration > 0 ? clip.mixOutCurve.GetHashCode() : 0;

            return new Hash128 { Value = new uint4((uint)A, (uint)B, (uint)C, (uint)D) };
        }

        /// <summary>
        /// Gets a single animation curve that represents the weighting curve of the entire clip
        /// </summary>
        /// <param name="clip"></param>
        /// <returns>null if the TimelineClip does not have any blend or ease weights</returns>
        public static AnimationCurve? CreateClipWeightCurve(this TimelineClip? clip)
        {
            if (clip == null)
            {
                return null;
            }

            if (clip.mixInDuration < float.Epsilon && clip.mixOutDuration < float.Epsilon)
            {
                return null;
            }

            // Add the mix in and mix out keys together. the curves should end and start at 1
            var keys = new List<Keyframe>(10);
            if (clip.mixInDuration >= float.Epsilon)
            {
                float2 range = new float2(
                    (float)clip.clipIn, (float)clip.ToLocalTime(clip.mixInDuration + clip.start)
                );
                var curve = clip.mixInCurve;

                // remap keys to local time
                var mixInKeys = curve.keys;
                for (int i = 0; i < mixInKeys.Length; i++)
                {
                    mixInKeys[i].time = (mixInKeys[i].time * (range.y - range.x)) + range.x;
                }

                keys.AddRange(mixInKeys);
            }

            if (clip.mixOutDuration >= float.Epsilon)
            {
                var range = new float2((float)clip.ToLocalTime(clip.end - clip.mixOutDuration), (float)clip.ToLocalTime(clip.end));
                var curve = clip.mixOutCurve;

                // remap keys to local time
                var mixOutKeys = curve.keys;
                for (int i = 0; i < mixOutKeys.Length; i++)
                {
                    mixOutKeys[i].time = (mixOutKeys[i].time * (range.y - range.x)) + range.x;
                }

                keys.AddRange(mixOutKeys);
            }

            if (keys.Count > 0)
            {
                return new AnimationCurve(keys.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Checks if a timeline clip is within range, including loops of the timeline asset.
        /// </summary>
        /// <param name="clip">The TimelineClip including Loops</param>
        /// <param name="activeRange">The active range to check. If this is larger than the timelines range, it is checked for loops</param>
        /// <returns></returns>
        public static bool InRangeInclLoops(this TimelineClip clip, ActiveRange activeRange)
        {
            var clipRange = clip.GetActiveRange();
            var timelineRange = default(ActiveRange);
            var parentTrack = clip.GetParentTrack();

            if (parentTrack != null && parentTrack.timelineAsset != null)
            {
                timelineRange = parentTrack.timelineAsset.GetRange();
            }

            return InRangeInclLoops(clipRange, activeRange, timelineRange);
        }

        /// <summary>
        /// Checks if a timeline clip is within range, including loops of the timeline asset.
        /// </summary>
        /// <param name="clipRange">The range of the TimelineClip including Loops</param>
        /// <param name="activeRange">The active range to check. If this is larger than the timelines range, it is checked for loops</param>
        /// <returns></returns>
        public static bool InRangeInclLoops(ActiveRange clipRange, ActiveRange activeRange, ActiveRange timelineRange)
        {
            if (!activeRange.IsValid())
            {
                return false;
            }

            // the range overlaps the clip
            if (clipRange.Overlaps(activeRange))
            {
                return true;
            }

            if (!timelineRange.IsValid())
            {
                return false;
            }

            // if the active range is larger, this clip needs to be included
            if (timelineRange.Length() < activeRange.Length())
            {
                return true;
            }

            // if the other range is completely contained in the timeline range
            if (timelineRange.Contains(activeRange))
            {
                return false;
            }

            // compute the range at the start and end of the clip that the loops represent
            var endRange = timelineRange;
            endRange.Start = activeRange.Start % timelineRange.Length();

            var startRange = timelineRange;
            startRange.End = activeRange.End % timelineRange.Length();

            return endRange.Overlaps(clipRange) || startRange.Overlaps(clipRange);
        }

        /// <summary>Returns the active range of the subtimeline of this clip</summary>
        public static ActiveRange GetSubTimelineRange(this TimelineClip clip)
        {
            return new ActiveRange()
            {
                Start = new DiscreteTime(clip.ToLocalTimeUnbound(clip.extrapolatedStart)),
                End = new DiscreteTime(clip.ToLocalTimeUnbound(clip.extrapolatedStart + clip.extrapolatedDuration)),
            };
        }

        /// <summary>Returns the extrapolated range of the timeline clip, clamped to the length of the timeline</summary>
        public static double2 GetEffectiveRange(this TimelineClip clip)
        {
            var s = clip.extrapolatedStart;
            var e = clip.extrapolatedStart + clip.extrapolatedDuration;

            var parentTrack = clip.GetParentTrack();

            if (parentTrack != null && parentTrack.timelineAsset != null)
            {
                e = math.min(e, parentTrack.timelineAsset.duration);
            }

            return new double2(s, e);
        }
    }
}
