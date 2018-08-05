using Nashet.EconomicSimulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Node.
/// </summary>
public class Node : MonoBehaviour
{

    /// <summary>
    /// The connections (neighbors).
    /// </summary>
    [SerializeField]
    protected List<Node> m_Connections = new List<Node>();

    [SerializeField]
    private Province province;

    public Province Province
    {
        get { return province; }
    }
    /// <summary>
    /// Gets the connections (neighbors).
    /// </summary>
    /// <value>The connections.</value>
    public virtual List<Node> connections
    {
        get
        {
            return m_Connections;
        }
    }

    public Node this[int index]
    {
        get
        {
            return m_Connections[index];
        }
    }

    void OnValidate()
    {
        // Removing duplicate elements
        m_Connections = m_Connections.Distinct().ToList();
    }
    public void Set(Province province, List<Node> m_Connections)
    {
        this.province = province;
        this.m_Connections = m_Connections;
    }
    public void Set(Province province, IEnumerable<Province> provinces)
    {
        this.province = province;
        foreach (var next in provinces)
            this.m_Connections.Add(next.GameObject.GetComponent<Node>());
    }
}
