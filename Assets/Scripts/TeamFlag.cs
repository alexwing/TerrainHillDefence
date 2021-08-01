using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamFlag : MonoBehaviour
{
    // material to change color of the flag
    public GameObject flag;
    public Color teamColor;

    // Use this for initialization
    void Start()
    {
        changeFlagColor(teamColor);
    }

    public Material flagMaterial;


    // change the color of the flag
    public void changeFlagColor(Color color)
    {
        //clone material
        Renderer newRenderer = flag.GetComponent<Renderer>();
        var propBlock = new MaterialPropertyBlock();
        newRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", color);
        propBlock.SetColor("_EmissionColor", Utils.Darken(color,0.25f));
        newRenderer.SetPropertyBlock(propBlock);


    }
}

