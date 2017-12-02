﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// this class handles all toon movement and interactions.
/// </summary>
/// 
public class ProtoMove : NetworkBehaviour {
    public bool selected; // if toon is selected
    public int speed = 10; // speed my toon is moving
    private bool delayed = true, justset = false; // delay so toon doesnt bounce
    GameObject attackarea = null; // for trigger check


    [SyncVar]
    public Vector3 moveto; // tell toons where to go 

    [SyncVar]
    public string owner = ""; // which player own me?

    [SyncVar]
    public bool canattack = false; // i can attack

    [SyncVar]
    public bool attacking = false; // i can attack

    [SyncVar]
    public bool canbeattack = false; // someone can attack me

    [SyncVar]
    public bool canmove = false; // i can move

    [SyncVar]
    public Color pcolor = Color.white; // my toon color 

    //solution no2 - initializing and controlling toon health from here instead
    [SyncVar]
    public int health = 5;

    //solution no2
    [ClientRpc]
    void RpcDamage(int newhealth)
    {
        Debug.Log("Toon got attacked! Health now: " + newhealth);
    }

    //solution no2
    public void TakeDamage(int gotHitDamage)
    {
        if (!isServer)
            return;

        health -= gotHitDamage;
        RpcDamage(health);
    }



    // Use this for initialization
    void Start () {
        StartCoroutine(DelayStart(2));
    }

    // late init so it can be alive and gravity kicks in first.
    public IEnumerator DelayStart(float time)
    {
        yield return new WaitForSeconds(time);
        delayed = false;
        moveto = transform.position;

    }
    

    void OnTriggerEnter(Collider col)
    {
        string s = col.gameObject.name; // name of the collider 

        if (s.StartsWith("attack"))
        {
            canbeattack = true;
            attackarea = col.gameObject;
        }
    }


    // Update is called once per frame
    void LateUpdate () {
        if (delayed) return; // only start after init.
        if (selected) // if i am selected then i am red or else i am set player color.
        {
            Renderer[] rends = gameObject.transform.GetChild(0).GetChild(0).GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
                r.material.color = Color.red;
        }
        else {
            Renderer[] rends = gameObject.transform.GetChild(0).GetChild(0).GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
                r.material.color = pcolor;
        }

        // if i am far from where i am moving to then look at it and keep moving. 
        // once i am close enough then froce set the position. 
        if (Vector3.Distance(transform.position, moveto) > 2f) {
            if (attacking && canattack) { transform.LookAt(moveto); canattack = false; justset = true; }
            else if (canmove && !attacking) { transform.LookAt(moveto); canmove = false; justset = true; }
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else if(justset){
            transform.position = new Vector3(moveto.x, 0, moveto.z); justset = false;
            if (attacking) attacking = false;
        }

        if (!attackarea) { // on trigger exit
            canbeattack = false;
        }
        
    }
}
