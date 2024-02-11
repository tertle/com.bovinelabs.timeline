// <copyright file="DOTSClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Playables;

    public abstract class DOTSClip : PlayableAsset
    {
         public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
         {
             return Playable.Create(graph);
         }

         /// <summary>
         /// Creates an entity from this clip, using the context object provided. Override this method to change the default
         /// clip create method, or return Entity.Null where a conversion is not required.
         /// </summary>
         public virtual Entity CreateClipEntity(BakingContext context)
         {
             return context.CreateClipEntity();
         }

         /// <summary> Override this method to add addition components to the clipEntity provided. </summary>
         public virtual void Bake(Entity clipEntity, BakingContext context)
         {
         }
    }
}
