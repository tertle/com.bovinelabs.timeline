// <copyright file="ActionTimelineBinding.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_REACTION
namespace BovineLabs.Timeline.Data.Reaction
{
    using BovineLabs.Reaction.Data.Core;
    using Unity.Collections;
    using Unity.Entities;

    [InternalBufferCapacity(0)]
    public struct ActionTimelineBinding : IBufferElementData
    {
        public FixedString32Bytes TrackIdentifier;
        public Target Target;
    }
}
#endif
