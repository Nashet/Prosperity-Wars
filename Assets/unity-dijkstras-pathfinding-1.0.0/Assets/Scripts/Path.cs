using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Path.
/// </summary>
public class Path
{

    /// <summary>
    /// The nodes.
    /// </summary>
    protected List<Node> m_Nodes = new List<Node>();

    /// <summary>
    /// The length of the path.
    /// </summary>
    protected float m_Length = 0f;

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    /// <value>The nodes.</value>
    public virtual List<Node> nodes
    {
        get
        {
            return m_Nodes;
        }
    }

    /// <summary>
    /// Gets the length of the path.
    /// </summary>
    /// <value>The length.</value>
    public virtual float length
    {
        get
        {
            return m_Length;
        }
    }

    /// <summary>
    /// Bake the path.
    /// Making the path ready for usage, Such as calculating the length.
    /// </summary>
    public virtual void Bake()
    {
        List<Node> calculated = new List<Node>();
        m_Length = 0f;
        for (int i = 0; i < m_Nodes.Count; i++)
        {
            Node node = m_Nodes[i];
            for (int j = 0; j < node.connections.Count; j++)
            {
                Node connection = node.connections[j];

                // Don't calculate calculated nodes
                if (m_Nodes.Contains(connection) && !calculated.Contains(connection))
                {

                    // Calculating the distance between a node and connection when they are both available in path nodes list
                    m_Length += Vector3.Distance(node.transform.position, connection.transform.position);
                }
            }
            calculated.Add(node);
        }
    }

    public Vector3[] GetVector3Nodes()
    {
        Vector3[] array = new Vector3[nodes.Count+1];
        for (int i = 0; i < nodes.Count; i++)
        {
            array[i+1] = nodes[i].Province.Position;
            array[i + 1].z = -2f;
        }
        return array;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString()
    {
        return string.Format(
            "Nodes: {0}\nLength: {1}",
            string.Join(
                ", ",
                nodes.Select(node => node.name).ToArray()),
            length);
    }

    public void Add(Path path)
    {
        if (path != null)
        {
            nodes.AddRange(path.nodes);
            Bake();
        }
    }
}
