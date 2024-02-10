// <copyright file="AnimatedComponent.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using BovineLabs.Core.Collections;
    using Unity.Entities;

    public interface IAnimatedComponent<T, TB> : IComponentData
        where T : unmanaged
        where TB : unmanaged, IBlobCurveSampler<T>
    {
        T DefaultValue { get; }

        TB AnimationData { get; }
    }

    // public struct AnimatedComponent<T> : IAnimatedComponent<T, BlobCurveSampler<T>>
    //     where T : unmanaged, IComponentData
    // {
    //     /// <summary> The default value of the component. </summary>
    //     public T DefaultValue { get; set; }
    //
    //     public BlobCurveSampler<T> AnimationData { get; set; }
    // }
    //
    // public struct AnimatedComponent2<T> : IAnimatedComponent<T, BlobCurveSampler2<T>>
    //     where T : unmanaged, IComponentData
    // {
    //     /// <summary> The default value of the component. </summary>
    //     public T DefaultValue { get; set; }
    //
    //     public BlobCurveSampler2<T> AnimationData { get; set; }
    // }
    //
    // public struct AnimatedComponent3<T> : IAnimatedComponent<T, BlobCurveSampler3<T>>
    //     where T : unmanaged, IComponentData
    // {
    //     /// <summary> The default value of the component. </summary>
    //     public T DefaultValue { get; set; }
    //
    //     public BlobCurveSampler3<T> AnimationData { get; set; }
    // }
}
