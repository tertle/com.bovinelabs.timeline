// <copyright file="TrackAssetExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System.Collections.Generic;
    using UnityEngine.Timeline;

    /// <summary>
    /// Extension methods for TrackAssets
    /// </summary>
    public static class TrackAssetExtensions
    {
        /// <summary>Get clips from all layers of a track, excluding any on muted tracks</summary>
        public static IEnumerable<TimelineClip> GetActiveClipsFromAllLayers(this TrackAsset asset)
        {
            if (asset.muted)
            {
                yield break;
            }

            foreach (var c in asset.GetClips())
            {
                yield return c;
            }

            foreach (var t in asset.GetChildTracks())
            {
                if (t.muted)
                {
                    continue;
                }

                foreach (var c in t.GetClips())
                {
                    yield return c;
                }
            }
        }
    }
}
