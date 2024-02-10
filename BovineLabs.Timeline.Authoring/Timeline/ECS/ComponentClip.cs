// <copyright file="ComponentClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using UnityEngine;
    using UnityEngine.Playables;

    public abstract class ComponentClip : DOTSClip
    {
        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return Playable.Create(graph);
        }
    }
}
