// <copyright file="TrackKeyBindings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;
    using Object = UnityEngine.Object;

    /// <summary> Serializable Track/Target Bindings. </summary>
    [Serializable]
    public struct TrackKeyBindings
    {
        /// <summary>Track->Tag pairs</summary>
        [Serializable]
        public struct TrackKeyPair
        {
            public TrackAsset Track;
            public Object Target;
        }

        /// <summary>The list of bindings. </summary>
        public List<TrackKeyPair> Bindings;

        /// <summary>Sync the track list to the given Timeline</summary>
        public void SyncToTimeline(TimelineAsset timeline)
        {
            if (this.Bindings == null)
            {
                this.Bindings = new List<TrackKeyPair>();
            }

            if (timeline != null)
            {
                var list = this.Bindings;

                var outputs = timeline.outputs.ToList();
                foreach (PlayableBinding output in outputs)
                {
                    var track = output.sourceObject as TrackAsset;
                    if (output.outputTargetType == null || track == null)
                    {
                        continue;
                    }

                    if (list.FindIndex(x => x.Track == track) == -1)
                    {
                        list.Add(new TrackKeyPair() { Track = track } );
                    }
                }

                for (int i = list.Count-1; i >= 0; i--)
                {
                    if (list[i].Track == null || outputs.FindIndex(o => o.sourceObject == list[i].Track) == -1)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>Given a track, find the corresponding tag. </summary>
        public Object? FindObject(TrackAsset? asset)
        {
            if (asset == null || this.Bindings == null)
            {
                return null;
            }

            int index = this.Bindings.FindIndex(x => x.Track == asset);
            return index >= 0 ? this.Bindings[index].Target : null;
        }
    }
}
