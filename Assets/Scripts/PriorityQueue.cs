using System.Collections;
using System.Collections.Generic;
public class PriorityQueue 
{
    private List<Node> nodes = new();
    
    public int Length
    {
        get { return nodes.Count; }
    }
    
    public bool Contains(Node node)
    {
        return nodes.Contains(node);
    }
    
    public Node GetFirstNode()
    {
        if (nodes.Count > 0)
        {
            return nodes[0];
        }
        return null;
    }
    
    public void Push(Node node)
    {
        nodes.Add(node);
        nodes.Sort();
    }

    public void Remove(Node node)
    {
        nodes.Remove(node);
        nodes.Sort();
    }

}


