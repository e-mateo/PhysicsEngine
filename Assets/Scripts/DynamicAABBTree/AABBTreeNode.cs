
using UnityEngine;

public class AABBTreeNode
{
    public AABBTreeNode parent;
    public AABBTreeNode childA;
    public AABBTreeNode childB;
    public AABB AABBBox;
    public Collider collider;
    public bool bHasCrossedChildren = false;

    public bool IsLeaf { get; private set; }

    public AABBTreeNode(AABB aabbBox, Collider collider, bool bIsLeaf)
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
