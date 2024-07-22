// <copyright file="ActionTimeline.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_REACTION
namespace BovineLabs.Timeline.Data.Reaction
{
    using Unity.Entities;

    public struct ActionTimeline : IComponentData
    {
        public float InitialTime;
        public bool Deactivate;
    }
}
#endif