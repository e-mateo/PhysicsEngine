
using UnityEngine;
namespace CustomPhysic
{
    public class AABBTreeNode
    {
        public AABBTreeNode parent;
        public AABBTreeNode childA;
        public AABBTreeNode childB;
        public AABB AABBBox;
        public CustomCollider collider;
        public bool bHasCrossedChildren = false;

        public bool IsLeaf { get; private set; }

        public AABBTreeNode(AABB aabbBox, CustomCollider collider, bool bIsLeaf)
        {
            this.AABBBox = aabbBox;
            this.collider = collider;
            this.IsLeaf = bIsLeaf;
        }

        public AABBTreeNode(bool bIsLeaf)
        {
            this.IsLeaf = bIsLeaf;
        }
    }
}
