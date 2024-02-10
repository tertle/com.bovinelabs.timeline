// <copyright file="DOTSClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public abstract class DOTSClip : PlayableAsset
    {
//        public struct AnimatedField
//         {
//             public string Name;
//             public AnimationCurve Curve;
//         }
//
//         struct AnimatedBlobs
//         {
//             public string Prefix;
//             public string Type;
//             public BlobAssetReference<AnimatedComponentBlob> Blob;
//             public int DirtyIndex;
//         }
//
        public TimelineClip? Clip { get; set; }

//
//         private int m_AnimClipDirtyIndex;
//
// #pragma warning disable 414
//         private int m_AnimFieldsDirtyIndex = -1;
// #pragma warning restore 414
//
//         private int m_LastDirtyFlag = -1;
//
//
//         public readonly List<AnimatedField> AnimatedFields = new List<AnimatedField>(10);
//         private readonly List<AnimatedBlobs> m_Blobs = new List<AnimatedBlobs>(5);
//
         public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
         {
             return Playable.Create(graph);
         }
//
//         protected virtual void OnEnable()
//         {
//             m_AnimClipDirtyIndex++;
//         }
//
//         protected virtual void OnDisable()
//         {
//             foreach (var e in m_Blobs)
//             {
//                 if (e.Blob != BlobAssetReference<AnimatedComponentBlob>.Null)
//                     e.Blob.Dispose();
//             }
//             m_Blobs.Clear();
//             m_AnimClipDirtyIndex++;
//         }
//
//
         /// <summary>
         /// Creates an entity from this clip, using the context object provided. Override this method to change the default
         ///   clip create method, or return Entity.Null where a conversion is not required
         /// </summary>
         /// <param name="context"></param>
         /// <returns></returns>
         public virtual Entity CreateClipEntity(BakingContext context)
         {
             return context.CreateClipEntity();
         }

         /// <summary>
         /// Override this method to add addition components to the clipEntity provided
         /// </summary>
         /// <param name="clipEntity"></param>
         /// <param name="context"></param>
         public virtual void Bake(Entity clipEntity, BakingContext context)
         {
         }
//
//
//         public virtual void OnCurvesModified(Entity entity, EntityManager dstManager)
//         {
//
//         }
//
//         public virtual void DeclareReferencedPrefabs(ConversionContext context, List<GameObject> referencedPrefabs)
//         {
//             //OVERRIDE ME
//         }
//
//         protected void ConvertCurves(AnimationClip clip)
//         {
// #if UNITY_EDITOR
//             if (clip == null || m_AnimClipDirtyIndex == m_AnimFieldsDirtyIndex)
//                 return;
//
//             AnimatedFields.Clear();
//
//             foreach (var binding in UnityEditor.AnimationUtility.GetCurveBindings(clip))
//             {
//                 var curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
//                 AnimatedFields.Add(new AnimatedField()
//                     {
//                         Name = binding.propertyName,
//                         Curve = curve
//                     }
//                 );
//             }
//
//             m_AnimFieldsDirtyIndex = m_AnimClipDirtyIndex;
// #endif
//         }
//
//         public BlobAssetReference<AnimatedComponentBlob> GetAnimationBlob<T>(string prefix) where T : struct, IComponentData
//         {
//             return GetAnimationBlob<T>(prefix, null);
//         }
//
//
//         public BlobAssetReference<AnimatedComponentBlob> GetAnimationBlob<T>(string prefix, Func<AnimationCurve,AnimationCurve> postProcessCurve) where T : struct, IComponentData
//         {
//             if (Clip == null)
//                 return BlobAssetReference<AnimatedComponentBlob>.Null;
//
//             for (int i = 0; i < m_Blobs.Count; i++)
//             {
//                 if (m_Blobs[i].Prefix == prefix && typeof(T).Name == m_Blobs[i].Type)
//                 {
//                     var entry = m_Blobs[i];
//                     if (entry.DirtyIndex == m_AnimClipDirtyIndex)
//                         return entry.Blob;
//
//                     ConvertCurves(Clip.curves);
//
//                     if (entry.Blob != BlobAssetReference<AnimatedComponentBlob>.Null)
//                         entry.Blob.Dispose();
//
//                     entry.Blob = ClipConversion.CreateAnimatedComponentBlob<T>(AnimatedFields, prefix, postProcessCurve);
//                     entry.DirtyIndex = m_AnimClipDirtyIndex;
//                     m_Blobs[i] = entry;
//
//                     return entry.Blob;
//                 }
//             }
//
//             ConvertCurves(Clip.curves);
//             var animatedBlob = new AnimatedBlobs()
//             {
//                 Prefix = prefix,
//                 Type = typeof(T).Name,
//                 Blob = ClipConversion.CreateAnimatedComponentBlob<T>(AnimatedFields, prefix, postProcessCurve),
//                 DirtyIndex = m_AnimClipDirtyIndex
//             };
//
//             m_Blobs.Add(animatedBlob);
//             return animatedBlob.Blob;
//         }
//
//         // tells the asset the animation clip needs to be reparsed
//         public void SetCurvesDirty()
//         {
//             m_AnimClipDirtyIndex++;
//         }
//
//         public bool CheckAndClearCurvesDirtyFlag()
//         {
//             bool dirty = m_LastDirtyFlag != m_AnimClipDirtyIndex;
//             m_LastDirtyFlag = m_AnimClipDirtyIndex;
//             return dirty;
//         }
    }
}
