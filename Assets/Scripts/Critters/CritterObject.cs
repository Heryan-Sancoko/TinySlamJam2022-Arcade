using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Critter", menuName = "Critter")]
public class CritterObject : ScriptableObject
{

    public int clipsize;
    public int currentAmmo;
    public float shotsPerSecond;

    //time before critter jumps out of the pen
    public float impatience;

    public AnimatorOverrideController mAnimController;
    public GameObject critterObj;

}
