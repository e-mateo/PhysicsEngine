using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsMaterial", menuName = "Physics/PhysicsMaterial", order = 1)]
public class CustomPhysicMaterial : ScriptableObject
{
    public float bouciness;
    public float friction;
}
