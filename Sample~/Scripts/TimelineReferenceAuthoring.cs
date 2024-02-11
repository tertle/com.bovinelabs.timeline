using Unity.Entities;
using UnityEngine;

public struct TimelineReference : IComponentData
{
}

public class TimelineReferenceAuthoring : MonoBehaviour
{
    private class Baker : Baker<TimelineReferenceAuthoring>
    {
        public override void Bake(TimelineReferenceAuthoring authoring)
        {
            this.AddComponent<TimelineReference>(this.GetEntity(TransformUsageFlags.None));
        }
    }
}
