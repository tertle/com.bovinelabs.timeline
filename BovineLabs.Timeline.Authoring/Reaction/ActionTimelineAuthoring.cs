// <copyright file="ActionTimelineAuthoring.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_REACTION
namespace BovineLabs.Timeline.Authoring.Reaction
{
    using System;
    using BovineLabs.Reaction.Authoring;
    using BovineLabs.Reaction.Authoring.Core;
    using BovineLabs.Reaction.Data.Core;
    using BovineLabs.Timeline.Data.Reaction;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Playables;

    [ReactionAuthoring]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ReactionAuthoring))]
    [RequireComponent(typeof(PlayableDirector))]
    public class ActionTimelineAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float initialTime;

        [SerializeField]
        private bool deactivate = true;

        [SerializeField]
        private Binding[] bindings = Array.Empty<Binding>();

        /// <inheritdoc />
        private class Baker : Baker<ActionTimelineAuthoring>
        {
            /// <inheritdoc />
            public override void Bake(ActionTimelineAuthoring authoring)
            {
                var entity = this.GetEntity(TransformUsageFlags.None);

                this.AddComponent(entity, new ActionTimeline
                {
                    InitialTime = authoring.initialTime,
                    Deactivate = authoring.deactivate,
                });

                var bindings = this.AddBuffer<ActionTimelineBinding>(entity);
                foreach (var binding in authoring.bindings)
                {
                    if (binding.Track == null)
                    {
                        continue;
                    }

                    this.DependsOn(binding.Track);

                    // No point binding if target is none
                    if (binding.Target == Target.None)
                    {
                        continue;
                    }

                    bindings.Add(new ActionTimelineBinding
                    {
                        TrackIdentifier = TimelineBakingUtility.TrackToIdentifier(binding.Track),
                        Target = binding.Target,
                    });
                }
            }
        }

        [Serializable]
        internal struct Binding
        {
            public DOTSTrack Track;
            public Target Target;
        }
    }
}
#endif
