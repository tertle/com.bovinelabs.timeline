// <copyright file="ActionTimelineAuthoringBindingEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_REACTION
namespace BovineLabs.Timeline.Editor.Reaction
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core.Editor.Inspectors;
    using BovineLabs.Timeline.Authoring;
    using BovineLabs.Timeline.Authoring.Reaction;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(ActionTimelineAuthoring.Binding))]
    public class ActionTimelineAuthoringBindingEditor : ElementProperty
    {
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            switch (property.name)
            {
                case nameof(ActionTimelineAuthoring.Binding.Track):

                    var trackField = CreatePropertyField(property, property.serializedObject);
                    trackField.SetEnabled(false);

                    // Multi selection we just show default field
                    if (property.serializedObject.targetObjects.Length != 1)
                    {
                        return trackField;
                    }

                    var ve = new VisualElement();

                    var options = GetAllTracks(property.serializedObject);
                    var names = options.Select(s => s.name).ToList();
                    var current = property.objectReferenceValue as DOTSTrack;
                    var defaultIndex = options.IndexOf(current);

                    var trackDropDown = new DropdownField(property.name, names, defaultIndex); // TODO default
                    trackDropDown.AddToClassList(DropdownField.alignedFieldUssClassName);

                    trackDropDown.RegisterValueChangedCallback(evt =>
                    {
                        var index = names.IndexOf(evt.newValue);
                        property.objectReferenceValue = options[index];
                        property.serializedObject.ApplyModifiedProperties();
                    });

                    ve.Add(trackDropDown);
                    ve.Add(trackField);

                    return ve;
            }

            return base.CreateElement(property);
        }

        /// <inheritdoc/>
        protected override string GetDisplayName(SerializedProperty property)
        {
            var track = property.FindPropertyRelative(nameof(ActionTimelineAuthoring.Binding.Track)).objectReferenceValue;
            return track == null ? "Null" : track.name;
        }

        private static List<DOTSTrack> GetAllTracks(SerializedObject serializedObject)
        {
            var tracks = new List<DOTSTrack>();

            if (serializedObject.targetObject is Component comp)
            {
                var director = comp.GetComponent<PlayableDirector>();
                GetTracksFromDirector(director, tracks);
            }

            return tracks;
        }

        private static void GetTracksFromDirector(PlayableDirector director, List<DOTSTrack> tracks)
        {
            if (director == null)
            {
                return;
            }

            var timeline = director.playableAsset as TimelineAsset;
            GetTracksFromTimeline(director, timeline, tracks);
        }

        private static void GetTracksFromTimeline(PlayableDirector director, TimelineAsset timeline, List<DOTSTrack> tracks)
        {
            if (timeline == null)
            {
                return;
            }

            var dotsTracks = timeline.GetDOTSTracks();

            foreach (var track in dotsTracks)
            {
                tracks.Add(track);

                if (track is SubDirectorTrack)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is SubDirectorClip subDirectorClip)
                        {
                            var subDirector = subDirectorClip.SubDirector.Resolve(director);
                            GetTracksFromDirector(subDirector, tracks);
                        }
                        else if (clip.asset is SubTimelineClip subTimelineClip)
                        {
                            GetTracksFromTimeline(director, subTimelineClip.Timeline, tracks);
                        }
                    }
                }
            }
        }
    }
}
#endif
