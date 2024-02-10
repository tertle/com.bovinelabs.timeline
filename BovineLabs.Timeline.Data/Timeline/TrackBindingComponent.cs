// <copyright file="TrackBindingComponent.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using Unity.Entities;

    /// <summary> Component assigned to a Clip entity indicating which entities it targets. </summary>
    public struct TrackBinding : IComponentData
    {
        public Entity Value;
    }
}
