using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class DAABBTree : MonoBehaviour
{
    AABBTreeNode RootNode = null;
    List<AABBTreeNode> NodeList = new List<AABBTreeNode>();

    private void Start()
    {
        //Remplace later with our own collider component ?
        SphereCollider[] colliders = FindObjectsOfType<SphereCollider>();

        foreach (SphereCollider collider in colliders)
        {
            AABB Box = new AABB(collider.bounds.min, collider.bounds.max);
            CreateLeafNode(Box);
        }
    }

    public void SetRootNode(AABBTreeNode node)
    { 
        RootNode = node;
    }

    public void CreateLeafNode(AABB AABB)
    {
        AABBTreeNode newLeafNode = new AABBTreeNode(AABB, true);
        NodeList.Add(newLeafNode);

        if (RootNode == null)
        {
            SetRootNode(newLeafNode);
            return;
        }

        AABBTreeNode bestSibling = FindBestSibling(newLeafNode);
        InsertLeaf(newLeafNode, bestSibling);
        RefitAABB(newLeafNode);
    }

    AABBTreeNode FindBestSibling(AABBTreeNode newLeafNode)
    { 
        AABBTreeNode BestSibling = RootNode;
        Queue<AABBTreeNode> PathNode = new Queue<AABBTreeNode>();
        PathNode.Enqueue(BestSibling);

        while (!BestSibling.bIsLeaf)
        {
            BestSibling = PickBest(BestSibling.childA, BestSibling.childB, newLeafNode);
            PathNode.Enqueue(BestSibling);
        }

        return BestSibling;
    }

    AABBTreeNode PickBest(AABBTreeNode A, AABBTreeNode B, AABBTreeNode newLeafNode)
    {
        float CostChildA = GetNodeCost(A, newLeafNode);
        float CostChildB = GetNodeCost(B, newLeafNode);

        return CostChildA < CostChildB ? A : B;
    }

    void InsertLeaf(AABBTreeNode newLeaf, AABBTreeNode sibling)
    {
        AABBTreeNode newInternalNode = new AABBTreeNode(false);
        NodeList.Add(newInternalNode);
        AABBTreeNode oldParent = sibling.parent;
        newInternalNode.parent = oldParent;
        newInternalNode.AABB = AABB.Merge(newLeaf.AABB, sibling.AABB);

        if(oldParent != null)
        {
            if (oldParent.childA == sibling)
                oldParent.childA = newInternalNode;
            else if(oldParent.childB == sibling)
                oldParent.childB = newInternalNode;
        }
        else //Sibling is the root
        {
            SetRootNode(newInternalNode);
        }

        newInternalNode.childA = sibling;
        newInternalNode.childB = newLeaf;
        sibling.parent = newInternalNode;
        newLeaf.parent = newInternalNode;
    }

    void RefitAABB(AABBTreeNode LeafNode)
    {
        AABBTreeNode NodeToRefit = LeafNode.parent.parent;
        while(NodeToRefit != null)
        {
            NodeToRefit.AABB = AABB.Merge(NodeToRefit.childA.AABB, NodeToRefit.childB.AABB);

            TreeRotation(NodeToRefit);

            NodeToRefit = NodeToRefit.parent;
        }
    }

    void TreeRotation(AABBTreeNode node)
    {

    }

    float GetTreeCost()
    {
        float cost = 0; 
        foreach(AABBTreeNode node in NodeList)
        {
            if(node.bIsLeaf == false)
            {
                cost += node.AABB.GetArea();
            }
        }
        return cost;
    }

    float GetNodeCost(AABBTreeNode Node, AABBTreeNode newLeaf)
    {
        float cost = AABB.GetAreaUnion(Node.AABB, newLeaf.AABB);
        AABBTreeNode parent = Node.parent;
        while(parent != null)
        {
            cost += AABB.GetAreaUnion(parent.AABB, newLeaf.AABB) - parent.AABB.GetArea();
            parent = parent.parent;
        }

        return cost;
    }

    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (AABBTreeNode node in NodeList)
        {
            DrawAABB(node.AABB);
        }
    }

    private void DrawAABB(AABB box)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(box.center, box.extend);
    }

    #endregion
}
