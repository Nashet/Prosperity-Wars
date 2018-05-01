using Nashet.EconomicSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private GameObject destination;
    //[SerializeField]
    //private Province position;
    // Use this for initialization

    [SerializeField]
    private int ID;

    [SerializeField]
    private GameObject SelectionPart;

    Animator m_Animator;
    private readonly static List<Unit> allUnits = new List<Unit>();
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        allUnits.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        //if (destination == null)
        //{
        //    m_Animator.SetFloat("Forward", 0f, 0.1f, Time.deltaTime);
        //}
        //else
        //{
        //    transform.LookAt(destination.transform, Vector3.back);
        //    var m_ForwardAmount = 1f;
        //    m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        //}
    }

    internal void SetPosition(Vector3 position)
    {
        transform.position = position;
        //destination = transform.parent.gameObject;
    }

    internal void SendTo(Province province)
    {
        var destination = province.getPosition();        
        this.transform.LookAt(destination, Vector3.back);
        var m_ForwardAmount = 1f;
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
    }
    public void Simulate()
    { }

    internal static Unit FindByID(int meshNumber)
    {
        return allUnits.Find(x => Int32.Parse(x.name) == meshNumber);
    }

    internal void OnClick()
    {
        if (Game.selectedUnits.Contains(this))
            DeSelect();
        else
            Select();
    }
    private void Select()
    {
        Game.selectedUnits.Add(this);
        SelectionPart.SetActive(true);
    }
    private void DeSelect()
    {
        Game.selectedUnits.Remove(this);
        SelectionPart.SetActive(false);
    }
}
