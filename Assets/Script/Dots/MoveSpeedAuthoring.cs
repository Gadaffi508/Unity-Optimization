using UnityEngine;
using Unity.Entities;
using System;

public class MoveSpeedAuthoring : MonoBehaviour
{
    public float speed = 5f;

    class Baker : Baker<MoveSpeedAuthoring>
    {
        public override void Bake(MoveSpeedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveSpeedComponent { speed = authoring.speed });
        }
    }
}
