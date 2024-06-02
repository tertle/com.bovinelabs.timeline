// <copyright file="PositionClipEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Editor
{
    using System;
    using BovineLabs.Core.Editor.Inspectors;
    using BovineLabs.Timeline.Authoring;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(PositionClip))]
    public class PositionClipEditor : ElementEditor
    {
        private SerializedProperty positionTypeProperty;

        private PropertyField positionTypeField;
        private PropertyField positionField;
        private PropertyField offsetTypeField;
        private PropertyField offsetField;
        private PropertyField targetField;

        protected override VisualElement CreateElement(SerializedProperty property)
        {
            switch (property.name)
            {
                case nameof(PositionClip.Type):
                    this.positionTypeProperty = property;
                    return this.positionTypeField = CreatePropertyField(property);

                case nameof(PositionClip.Position):
                    return this.positionField = CreatePropertyField(property);

                case nameof(PositionClip.OffsetType):
                    return this.offsetTypeField = CreatePropertyField(property);

                case nameof(PositionClip.Offset):
                    return this.offsetField = CreatePropertyField(property);

                case nameof(PositionClip.Target):
                    return this.targetField = CreatePropertyField(property);
            }

            return base.CreateElement(property);
        }

        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            this.positionTypeField.RegisterValueChangeCallback(this.Callback);
            this.SetVisibility(this.positionTypeProperty);
        }

        private void Callback(SerializedPropertyChangeEvent evt)
        {
            this.SetVisibility(evt.changedProperty);
        }

        private void SetVisibility(SerializedProperty property)
        {
            ElementUtility.SetVisible(this.positionField, false);
            ElementUtility.SetVisible(this.offsetTypeField, false);
            ElementUtility.SetVisible(this.offsetField, false);
            ElementUtility.SetVisible(this.targetField, false);

            switch ((PositionType)property.enumValueIndex)
            {
                case PositionType.World:
                    ElementUtility.SetVisible(this.positionField, true);
                    break;
                case PositionType.Offset:
                    ElementUtility.SetVisible(this.offsetTypeField, true);
                    ElementUtility.SetVisible(this.offsetField, true);
                    break;
                case PositionType.Target:
                    ElementUtility.SetVisible(this.offsetTypeField, true);
                    ElementUtility.SetVisible(this.offsetField, true);
                    ElementUtility.SetVisible(this.targetField, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
