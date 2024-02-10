// <copyright file="JobHelpers.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    public static class JobHelpers
    {
        public static void AnimateUnblendExecuteGeneric<T, TB, TC>(
            in TrackBinding binding,
            in LocalTime localTime,
            ref TC animatedComponent,
            NativeParallelHashMap<Entity, MixData<T>>.ParallelWriter blendData)
            where T : unmanaged
            where TB : unmanaged, IBlobCurveSampler<T>
            where TC : unmanaged, IAnimatedComponent<T, TB>
        {
            if ((float)localTime.Value < 0)
            {
                return;
            }

            var v = animatedComponent.DefaultValue;
            if (animatedComponent.AnimationData.IsCreated)
            {
                v = animatedComponent.AnimationData.Evaluate((float)localTime.Value);
            }

            var mixData = new MixData<T>
            {
                Value1 = v,
                Weights = new float4(1, 0, 0, 0),
            };

            blendData.TryAdd(binding.Value, mixData);
        }

        public static void AccumulateWeightedAnimationExecuteGeneric<T, TB, TC>(
            in TrackBinding binding,
            in LocalTime localTime,
            ref TC animatedComponent,
            in ClipWeight c3,
            NativeParallelHashMap<Entity, MixData<T>> blendData)
            where T : unmanaged
            where TB : unmanaged, IBlobCurveSampler<T>
            where TC : unmanaged, IAnimatedComponent<T, TB>
        {
            if ((float)localTime.Value < 0)
            {
                return;
            }

            T v = animatedComponent.DefaultValue;

            if (animatedComponent.AnimationData.IsCreated)
            {
                v = animatedComponent.AnimationData.Evaluate((float)localTime.Value);
            }

            if (!blendData.TryGetValue(binding.Value, out var data))
            {
                data.Weights = float4.zero;
            }

            if (c3.Value > data.Weights.x)
            {
                data.Weights = data.Weights.xxyz;
                data.Weights.x = c3.Value;
                data.Value4 = data.Value3;
                data.Value3 = data.Value2;
                data.Value2 = data.Value1;
                data.Value1 = v;
            }
            else if (c3.Value > data.Weights.y)
            {
                data.Weights = data.Weights.xxyz;
                data.Weights.y = c3.Value;
                data.Value4 = data.Value3;
                data.Value3 = data.Value2;
                data.Value2 = v;
            }
            else if (c3.Value > data.Weights.z)
            {
                data.Weights = data.Weights.xyyz;
                data.Weights.z = c3.Value;
                data.Value4 = data.Value3;
                data.Value3 = v;
            }
            else if (c3.Value > data.Weights.w)
            {
                data.Weights.w = c3.Value;
                data.Value4 = v;
            }

            blendData[binding.Value] = data;
        }

        public static T Blend<T, TMixer>(ref MixData<T> values, in T defaultValue)
            where T : unmanaged
            where TMixer : unmanaged, IMixer<T>
        {
            const float epsilon = 1e-6f;
            T result = defaultValue;

            var mixer = new TMixer();
            if (values.Weights.x > epsilon)
            {
                var totalWeight = math.dot(values.Weights, new float4(1));
                if (totalWeight < 1 && !values.Additive)
                {
                    if (values.Weights.y <= epsilon)
                    {
                        values.Weights.y = 1 - totalWeight;
                        values.Value2 = defaultValue;
                    }
                    else if (values.Weights.z <= epsilon)
                    {
                        values.Weights.z = 1 - totalWeight;
                        values.Value3 = defaultValue;
                    }
                    else if (values.Weights.w <= epsilon)
                    {
                        values.Weights.w = 1 - totalWeight;
                        values.Value4 = defaultValue;
                    }

                    totalWeight = 1;
                }

                var weights = values.Weights * math.rcp(totalWeight);
                if (weights.y <= epsilon)
                {
                    result = values.Value1;
                }
                else if (weights.z <= epsilon)
                {
                    result = mixer.Lerp(values.Value1, values.Value2, weights.y);
                }
                else
                {
                    var w = weights.x + weights.y;
                    var a = mixer.Lerp(values.Value1, values.Value2, weights.y / w);
                    var b = mixer.Lerp(values.Value3, values.Value4, weights.w / (1 - w));
                    result = mixer.Lerp(b, a, w);
                }

                if (values.Additive)
                {
                    result = mixer.Add(defaultValue, result);
                }
            }

            return result;
        }
    }
}
