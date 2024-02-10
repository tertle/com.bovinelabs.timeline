// <copyright file="IMixer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using Unity.Entities;

    public interface IMixer<T> : IComponentData
        where T : unmanaged
    {
        T Lerp(in T a, in T b, float s);

        T Add(in T a, in T b);
    }
}
