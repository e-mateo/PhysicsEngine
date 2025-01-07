using System.Collections.Generic;
using UnityEngine;

public struct CollisionPair
{
    //TODO: Remplace with RigidBody ?
    public CustomCollider colliderA;
    public CustomCollider colliderB;

    public CollisionPair(CustomCollider A, CustomCollider B)
    {
        colliderA = A;
        colliderB = B;
    }
}

public class DAABBTree : MonoBehaviour
{
    private AABBTreeNode rootNode = null;
    private List<AABBTreeNode> nodes = new List<AABBTreeNode>();
    private Dictionary<CustomCollider, AABBTreeNode> leafNodes = new Dictionary<CustomCollider, AABBTreeNode>();

    private List<CollisionPair> collisionPairs = new List<CollisionPair>();

    public void AddColliderToTree(CustomCollider collider)
    {
        CreateLeafNode(collider);
    }

    public void RemoveColliderFromTree(CustomCollider collider)
    {
        RemoveLeafNode(collider);
    }

    public void UpdateTreeAndCollisionPairs()
    {
        UpdateABBBTree();
        UpdateCollisionPairs();
    }
    public List<CollisionPair> GetCollisionPairs()
    {
        return collisionPairs;
    }

    private void UpdateABBBTree()
    {
        List <KeyValuePair <CustomCollider, AABBTreeNode>> nodeToUpdate = new List<KeyValuePair<CustomCollider, AABBTreeNode>>();
        foreach (KeyValuePair<CustomCollider, AABBTreeNode> node in leafNodes)
        {
            AABBTreeNode treeNode = node.Value;
            treeNode.AABBBox.UpdateAABB(treeNode.collider.worldBounds.center);
            if (treeNode.AABBBox.HasExitEnlargedAABB())
            {
                treeNode.AABBBox.UpdateEnlargedAABB();
                nodeToUpdate.Add(node);
            }
        }

        for (int i = 0; i < nodeToUpdate.Count; i++)
        {
            RemoveLeafNode(nodeToUpdate[i].Key);
            CreateLeafNode(nodeToUpdate[i].Value.collider);
        }
    }

    private void SetRootNode(AABBTreeNode node)
    { 
        rootNode = node;
    }

    private void CreateLeafNode(CustomCollider collider)
    {
        AABB AABBBox = new AABB(collider.worldBounds.center, collider.worldBounds.extents);
        AABBTreeNode newLeafNode = new AABBTreeNode(AABBBox, collider, true);
        nodes.Add(newLeafNode);
        leafNodes.Add(collider, newLeafNode);

        if (rootNode == null)
        {
            SetRootNode(newLeafNode);
            return;
        }

        AABBTreeNode bestSibling = FindBestSibling(newLeafNode);
        InsertLeaf(newLeafNode, bestSibling);
        RefitAABB(newLeafNode);
    }

    private void RemoveLeafNode(CustomCollider collider)
    {
        AABBTreeNode treeNode = leafNodes[collider];
        if (treeNode == null) { return;}

        leafNodes.Remove(collider);
        nodes.Remove(treeNode);

        if(treeNode.parent != null)
        {
            AABBTreeNode parentNode = treeNode.parent;
            AABBTreeNode sibling = parentNode.childA == treeNode ? parentNode.childB : parentNode.childA;

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
                rootNode = sibling;
            }

            nodes.Remove(parentNode);
            RefitAABB(sibling);
        }
        else
        {
            rootNode = null;
        }

    }
    private AABBTreeNode FindBestSibling(AABBTreeNode newLeafNode)
    {
        Queue<AABBTreeNode> priorityQueue = new Queue<AABBTreeNode>();
        priorityQueue.Enqueue(rootNode);
        AABBTreeNode bestSibling = rootNode;
        float lowestCost = AABB.GetAreaUnion(newLeafNode.AABBBox, rootNode.AABBBox);
        
        while (priorityQueue.Count != 0)
        {
            AABBTreeNode possibleSibling = priorityQueue.Dequeue();
            if (possibleSibling != null)
            {
                float cost = GetNodeCostIfSelectedAsSibling(possibleSibling, newLeafNode);
                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    bestSibling = possibleSibling;
                }

                if (IsWorthToExploreBranch(possibleSibling, newLeafNode, lowestCost))
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
    
    private bool IsWorthToExploreBranch(AABBTreeNode branchNode, AABBTreeNode newLeafNode, float currentBestCost)
    {
        //Acts as if the newLeafNode become a child of branchNode
        float minCostIfExploringBranch = newLeafNode.AABBBox.GetArea();
        AABBTreeNode parent = branchNode;
        while (parent != null)
        {
            minCostIfExploringBranch += AABB.GetAreaUnion(parent.AABBBox, newLeafNode.AABBBox) - parent.AABBBox.GetArea();
            parent = parent.parent;
        }

        return minCostIfExploringBranch < currentBestCost;
    }

    void InsertLeaf(AABBTreeNode newLeaf, AABBTreeNode sibling)
    {
        AABBTreeNode newInternalNode = new AABBTreeNode(false);
        nodes.Add(newInternalNode);
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

    void RefitAABB(AABBTreeNode LeafNode)
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

    bool TreeRotation(AABBTreeNode nodeToRefit)
    {
        if(nodeToRefit.parent == null) return false;

        AABBTreeNode parentNode = nodeToRefit.parent;

        if (TrySwitch(parentNode.childA, parentNode.childB.childA))
        {
            return true;
        }
        if (TrySwitch(parentNode.childA, parentNode.childB.childB))
        {
            return true;
        }
        if (TrySwitch(parentNode.childB, parentNode.childA.childA))
        {
            return true;
        }
        if (TrySwitch(parentNode.childB, parentNode.childA.childB))
        {
            return true;
        }

        return false;
    }

    bool TrySwitch(AABBTreeNode From, AABBTreeNode To) //From need to be higher in the tree than To
    {
        if (From == null || To == null)
        {
            return false;
        }

        AABBTreeNode To_Sibling = (To == To.parent.childA) ? To.parent.childB : To.parent.childA;

        //Check if the cost of B Parent become lower if A is inverted with B
        float To_costParent = To.parent.AABBBox.GetArea();
        float To_newCostParent = AABB.Merge(From.AABBBox, To_Sibling.AABBBox).GetArea();

        if (To_newCostParent < To_costParent)
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


    public void UpdateCollisionPairs()
    {
        collisionPairs.Clear();

        if(rootNode == null || rootNode.IsLeaf)
        {
            return;
        }

        ResetHasCrossedChildren(rootNode);
        CheckCollisionPair(rootNode.childA, rootNode.childB);
    }

    private void ResetHasCrossedChildren(AABBTreeNode node)
    {
       if(node != null)
       {
            node.bHasCrossedChildren = false;
            ResetHasCrossedChildren(node.childA);
            ResetHasCrossedChildren(node.childB);
       }
    }

    private void CheckCollisionPair(AABBTreeNode nodeA, AABBTreeNode nodeB)
    {
        bool AABBCollides = AABB.IsColliding(nodeA.AABBBox, nodeB.AABBBox);

        if (nodeA.IsLeaf && nodeB.IsLeaf)
        {
            if(AABBCollides)
            {
                collisionPairs.Add(new CollisionPair(nodeA.collider, nodeB.collider));
            }
        }
        else if(nodeA.IsLeaf)
        {
            CrossChildren(nodeB);

            if (AABBCollides)
            {
                CheckCollisionPair(nodeA, nodeB.childA);
                CheckCollisionPair(nodeA, nodeB.childB);
            }
        }
        else if(nodeB.IsLeaf)
        {
            if (AABBCollides)
            {
                CheckCollisionPair(nodeB, nodeA.childA);
                CheckCollisionPair(nodeB, nodeA.childB);
            }

            CrossChildren(nodeA);
        }
        else
        {
            if (AABBCollides)
            {
                CheckCollisionPair(nodeA.childA, nodeB.childA);
                CheckCollisionPair(nodeA.childA, nodeB.childB);
                CheckCollisionPair(nodeA.childB, nodeB.childA);
                CheckCollisionPair(nodeA.childB, nodeB.childB);
            }

            CrossChildren(nodeA);
            CrossChildren(nodeB);
        }
    }

    private void CrossChildren(AABBTreeNode node)
    {
        if (!node.bHasCrossedChildren)
        {
            node.bHasCrossedChildren = true;
            CheckCollisionPair(node.childA, node.childB);
        }
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (AABBTreeNode node in nodes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(node.AABBBox.Center, node.AABBBox.Extend * 2f);
        }

        foreach (KeyValuePair<CustomCollider, AABBTreeNode> leaves in leafNodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(leaves.Value.AABBBox.CenterEnlargedAABB, leaves.Value.AABBBox.ExtendEnlargedAABB * 2f);
        }


        foreach(CollisionPair pair in collisionPairs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pair.colliderA.worldBounds.center, pair.colliderA.worldBounds.extents * 2f);
            Gizmos.DrawWireCube(pair.colliderB.worldBounds.center, pair.colliderB.worldBounds.extents * 2f);
        }
    }
    #endregion
}
