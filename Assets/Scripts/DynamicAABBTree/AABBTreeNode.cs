
public class AABBTreeNode
{
    public AABBTreeNode parent;
    public AABBTreeNode childA;
    public AABBTreeNode childB;
    public AABB AABBBox;
    public bool IsLeaf { get; private set; }

    public AABBTreeNode(AABB aabbBox, bool bIsLeaf)
    {
        this.AABBBox = aabbBox;
        this.IsLeaf = bIsLeaf;
    }

    public AABBTreeNode(bool bIsLeaf)
    {
        this.IsLeaf = bIsLeaf;
    }
}
