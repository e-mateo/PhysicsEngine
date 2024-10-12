using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AABBTreeNode
{
    public AABBTreeNode parent;

    public AABBTreeNode childA;
    public AABBTreeNode childB;

    public AABB AABB;

    public bool bIsLeaf;

    public AABBTreeNode(AABB AABB, bool bIsLeaf)
    {
        this.AABB = AABB;
        this.bIsLeaf = bIsLeaf;
    }

    public AABBTreeNode(bool bIsLeaf)
    {
        this.bIsLeaf = bIsLeaf;
    }


}
