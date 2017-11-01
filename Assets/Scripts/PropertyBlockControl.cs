using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PropertyBlockControl : MonoBehaviour {

    //We want each object to explode and color a little different, so we set this set property with a random float in Awake() to offset the noise function
    protected float _seed;
	[HideInInspector] public Renderer[] _renderers;
	protected MaterialPropertyBlock _propBlock;

    

	void Awake()
	{
		_propBlock = new MaterialPropertyBlock(); //first we create an empty material property block
		_seed = Random.Range (0f, 10f);
	}
	
}

