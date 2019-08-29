using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Click : MonoBehaviour {
    public Button table, vase, chair2, chair1;
    public GameObject obChair1, obChair2, obVase, obTable, obArmChair, obMultiTable;
    public bool c1, c2, v, t, ac, mt;
    void Start()
    {
        c1 = false;
        c2 = false;
        v = false;
        t = false;
        ac = false;
        mt = false;
        //Calls the TaskOnClick/TaskWithParameters/ButtonClicked method when you click the Button
       
    }
    private void Update()
    {
        /*table.onClick.AddListener(TaskOnClick4);
        vase.onClick.AddListener(TaskOnClick3);
        chair2.onClick.AddListener(TaskOnClick2);
        chair1.onClick.AddListener(TaskOnClick1);*/
    }
    public void TaskOnClick1()
    {

        if (c1)
        {
            obChair1.SetActive(false);
            c1 = false;
        }
        else
        {
            obChair1.SetActive(true);
            c1 = true;
        }
        
       
    }

    void TaskOnClick2()
    {
        if (c2)
        {
            obChair1.SetActive(false);
        }
        else
        {
            obChair1.SetActive(true);
            c2 = true;
        }

    }
    void TaskOnClick3()
    {
        if (v)
        {
            obChair1.SetActive(false);
        }
        else
        {
            obChair1.SetActive(true);
            v = true;
        }

    }
    public void TaskOnClick4()
    {
        if (t)
        {
            obChair1.SetActive(false);
            t = false;
        }
        else
        {
            obChair1.SetActive(true);
            t = true;
        }

    }


}