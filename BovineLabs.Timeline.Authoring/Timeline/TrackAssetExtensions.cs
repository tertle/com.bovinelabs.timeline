// <copyright file="TrackAssetExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Timeline.Data.Schedular;
    using Unity.IntegerTime;
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

        // /// <summary>Get clips from all layers of a track, even if muted</summary>
        // public static IEnumerable<TimelineClip> GetClipsFromAllLayers(this TrackAsset asset)
        // {
        //     foreach (var c in asset.GetClips())
        //     {
        //         yield return c;
        //     }
        //
        //     foreach (var t in asset.GetChildTracks())
        //     {
        //          foreach (var c in t.GetClips())
        //         {
        //             yield return c;
        //         }
        //     }
        // }
        //
        // /// <summary>Returns the main track. If this track is a subtrack, will return it's parent</summary>
        // public static TrackAsset? GetMainTrack(this TrackAsset? asset)
        // {
        //     if (asset == null)
        //     {
        //         return null;
        //     }
        //
        //     if (asset.isSubTrack)
        //     {
        //         return GetMainTrack(asset.parent as TrackAsset);
        //     }
        //
        //     return asset;
        // }

        // /// <summary>Returns the layer index of this in it's parent track</summary>
        // public static int GetLayerIndex(this TrackAsset asset)
        // {
        //     if (asset == null)
        //     {
        //         throw new ArgumentNullException(nameof(asset));
        //     }
        //
        //     var mainTrack = asset.GetMainTrack();
        //     if (mainTrack == asset)
        //     {
        //         return 0;
        //     }
        //
        //     int i = 1;
        //     foreach (var c in mainTrack.GetChildTracks())
        //     {
        //         if (asset == c)
        //         {
        //             return i;
        //         }
        //
        //         i++;
        //     }
        //
        //     return int.MinValue;
        // }

        // /// <summary>Gets the index of the track in the timeline</summary>
        // /// <returns>
        // /// The index of the track in the list of output tracks, or -1 if the track is a type not attached to an output track, such as a group track.
        // /// </returns>
        // public static int GetTrackOutputIndex(this TrackAsset asset)
        // {
        //     var mainTrack = GetMainTrack(asset);
        //     var outputs = asset.timelineAsset.GetOutputTracks();
        //     int i = 0;
        //     foreach (var track in outputs)
        //     {
        //         if (track == mainTrack)
        //         {
        //             return i;
        //         }
        //
        //         i++;
        //     }
        //
        //     return -1;
        // }

        // /// <summary>
        // /// Returns all markers on a track that occur within the given range
        // /// </summary>
        // /// <param name="asset"></param>
        // /// <param name="range"></param>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public static IEnumerable<T> GetMarkersInRange<T>(this TrackAsset asset, ActiveRange range)
        //     where T : IMarker
        // {
        //     if (asset == null)
        //         throw new ArgumentNullException(nameof(asset));
        //
        //     foreach (var m in asset.GetMarkers())
        //     {
        //         if (m is T && range.Contains(new DiscreteTime(m.time)))
        //             yield return (T) m;
        //     }
        // }
    }
}
