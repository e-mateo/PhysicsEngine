using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DAABBTree : MonoBehaviour
{
    AABBTreeNode RootNode = null;
    List<AABBTreeNode> NodeList = new List<AABBTreeNode>();
    List<AABBTreeNode> LeafNodeList = new List<AABBTreeNode>();

    static DAABBTree instance = null;
    static public DAABBTree Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<DAABBTree>();
            return instance;
        }
    }

    private void Start()
    {
        SphereCollider[] colliders = FindObjectsOfType<SphereCollider>();
        foreach (SphereCollider sphereCollider in colliders)
        {
            AABB Box = new AABB(sphereCollider.bounds.center, sphereCollider.bounds.extents);
            CreateLeafNode(Box, sphereCollider);
        }
    }

    private void Update()
    {
        List<AABBTreeNode> nodeToUpdate = new List<AABBTreeNode>();
        foreach (AABBTreeNode node in LeafNodeList)
        {
            node.AABBBox.UpdateAABB(node.collider.bounds.center);
            if (node.AABBBox.HasExitEnlargedAABB())
            {
                node.AABBBox.UpdateEnlargedAABB();
                nodeToUpdate.Add(node); 
            }
        }

        for(int i = 0; i < nodeToUpdate.Count; i++)
        {
            RemoveLeafNode(nodeToUpdate[i]);
            CreateLeafNode(nodeToUpdate[i].AABBBox, nodeToUpdate[i].collider);
        }
    }

    public void SetRootNode(AABBTreeNode node)
    { 
        RootNode = node;
    }

    public void CreateLeafNode(AABB AABB, Collider collider)
    {
        AABBTreeNode newLeafNode = new AABBTreeNode(AABB, collider, true);
        NodeList.Add(newLeafNode);
        LeafNodeList.Add(newLeafNode);

        if (RootNode == null)
        {
            SetRootNode(newLeafNode);
            return;
        }

        AABBTreeNode bestSibling = FindBestSibling(newLeafNode);
        InsertLeaf(newLeafNode, bestSibling);
        RefitAABB(newLeafNode);
    }

    public void RemoveLeafNode(AABBTreeNode LeafNode)
    {
        NodeList.Remove(LeafNode);
        LeafNodeList.Remove(LeafNode);

        if(LeafNode.parent != null)
        {
            AABBTreeNode parentNode = LeafNode.parent;
            AABBTreeNode sibling = parentNode.childA == LeafNode ? parentNode.childB : parentNode.childA;

            sibling.parent = parentNode.parent;
            if(sibling.parent != null)
            {
                if(sibling.parent.childA == parentNode)
                {
                    sibling.parent.childA = sibling;
                }
                else
                {
                    sibling.parent.childB = sibling;
                }
            }
            else
            {
                RootNode = sibling;
            }

            NodeList.Remove(parentNode);
            RefitAABB(sibling, false);
        }
        else
        {
            RootNode = null;
        }

    }

    AABBTreeNode FindBestSibling(AABBTreeNode newLeafNode)
    {
        Queue<AABBTreeNode> priorityQueue = new Queue<AABBTreeNode>();
        priorityQueue.Enqueue(RootNode);
        AABBTreeNode bestSibling = RootNode;
        float lowerCost = AABB.GetAreaUnion(newLeafNode.AABBBox, RootNode.AABBBox);
        
        while (priorityQueue.Count != 0)
        {
            AABBTreeNode possibleSibling = priorityQueue.Dequeue();
            if (possibleSibling != null)
            {
                float cost = GetNodeCostIfSelectedAsSibling(possibleSibling, newLeafNode);
                if (cost < lowerCost)
                {
                    lowerCost = cost;
                    bestSibling = possibleSibling;
                }

                if (IsItWorthToExploreBranch(possibleSibling, newLeafNode, lowerCost))
                {
                    priorityQueue.Enqueue(possibleSibling.childA);
                    priorityQueue.Enqueue(possibleSibling.childB);
                }
            }
        }
        
        return bestSibling;
    }

    float GetNodeCostIfSelectedAsSibling(AABBTreeNode possibleSibling, AABBTreeNode newLeaf)
    {
        float cost = AABB.GetAreaUnion(possibleSibling.AABBBox, newLeaf.AABBBox);
        AABBTreeNode parent = possibleSibling.parent;
        while(parent != null)
        {
            cost += AABB.GetAreaUnion(parent.AABBBox, newLeaf.AABBBox) - parent.AABBBox.GetArea();
            parent = parent.parent;
        }

        return cost;
    }
    
    private bool IsItWorthToExploreBranch(AABBTreeNode startBranch, AABBTreeNode newLeafNode, float bestCost)
    {
        float minCostIfExploringBranch = newLeafNode.AABBBox.GetArea();
        AABBTreeNode inheritedNode = startBranch;
        while (inheritedNode != null)
        {
            minCostIfExploringBranch += AABB.GetAreaUnion(inheritedNode.AABBBox, newLeafNode.AABBBox) - inheritedNode.AABBBox.GetArea();
            inheritedNode = inheritedNode.parent;
        }

        return minCostIfExploringBranch < bestCost;
    }

    void InsertLeaf(AABBTreeNode newLeaf, AABBTreeNode sibling)
    {
        AABBTreeNode newInternalNode = new AABBTreeNode(false);
        NodeList.Add(newInternalNode);
        AABBTreeNode oldParent = sibling.parent;
        newInternalNode.parent = oldParent;

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

    void RefitAABB(AABBTreeNode LeafNode, bool AllowTreeRotation = true)
    {
        AABBTreeNode NodeToRefit = LeafNode.parent;
        while(NodeToRefit != null)
        {
            NodeToRefit.AABBBox = AABB.Merge(NodeToRefit.childA.AABBBox, NodeToRefit.childB.AABBBox);

            if (TreeRotation(NodeToRefit))
            {
                NodeToRefit.AABBBox = AABB.Merge(NodeToRefit.childA.AABBBox, NodeToRefit.childB.AABBBox);
            }
            NodeToRefit = NodeToRefit.parent;
        }
    }

    bool TreeRotation(AABBTreeNode node)
    {
        if(node.parent == null) return false;
        if (TrySwitch(node.parent.childA, node.parent.childB.childA))
        {
            return true;
        }
        if (TrySwitch(node.parent.childA, node.parent.childB.childB))
        {
            return true;
        }
        if (TrySwitch(node.parent.childB, node.parent.childA.childA))
        {
            return true;
        }
        if (TrySwitch(node.parent.childB, node.parent.childA.childA))
        {
            return true;
        }

        return false;
    }

    bool TrySwitch(AABBTreeNode From, AABBTreeNode To) //From need to be higher in the tree than To
    {
        if (From == null || To == null)
            return false;

        float currentCostBParent = To.parent.AABBBox.GetArea();
        AABBTreeNode BSibling = To == To.parent.childA ? To.parent.childB : To.parent.childA;
        float newCostBParent = AABB.Merge(From.AABBBox, BSibling.AABBBox).GetArea();

        if (newCostBParent < currentCostBParent)
        {
            SwitchNodes(From, To);
            return true;
        }

        return false;
    }

    void SwitchNodes(AABBTreeNode From, AABBTreeNode To)
    {
        AABBTreeNode parentA = From.parent;
        AABBTreeNode parentB = To.parent;
        From.parent = parentB;
        To.parent = parentA;

        if (From.parent.childA == To)
        {
            From.parent.childA = From;
        }
        else
        {
            From.parent.childB = From;
        }
        
        if (To.parent.childA == From)
        {
            To.parent.childA = To;
        }
        else
        {
            To.parent.childB = To;
        }
        
        From.parent.AABBBox = AABB.Merge(From.parent.childA.AABBBox, From.parent.childB.AABBBox);
        To.parent.AABBBox = AABB.Merge(To.parent.childA.AABBBox, To.parent.childB.AABBBox);
    }

    float GetTreeCost()
    {
        float cost = 0; 
        foreach(AABBTreeNode node in NodeList)
        {
            if(node.IsLeaf == false)
            {
                cost += node.AABBBox.GetArea();
            }
        }
        return cost;
    }



    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (AABBTreeNode node in NodeList)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(node.AABBBox.Center, node.AABBBox.Extend * 2f);
        }

        foreach (AABBTreeNode leaves in LeafNodeList)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(leaves.AABBBox.CenterEnlargedAABB, leaves.AABBBox.ExtendEnlargedAABB * 2f);
        }
    }

    #endregion
}
