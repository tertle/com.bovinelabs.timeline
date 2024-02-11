// <copyright file="TimelineAssetExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.IntegerTime;
    using UnityEngine.Timeline;

    public static class TimelineAssetExtensions
    {
        /// <summary> Get all the DOTS-compatible tracks from a timeline </summary>
        public static IEnumerable<DOTSTrack> GetDOTSTracks(this TimelineAsset asset)
        {
            if (asset == null)
            {
                return Enumerable.Empty<DOTSTrack>();
            }

            return asset.GetOutputTracks().OfType<DOTSTrack>().Where(x => !x.mutedInHierarchy).ToList();
        }

        /// <summary> Get the active range of the timeline asset. </summary>
        public static ActiveRange GetRange(this TimelineAsset asset)
        {
            return new ActiveRange
            {
                Start = DiscreteTime.Zero,
                End = new DiscreteTime(asset.duration),
            };
        }
    }
}
