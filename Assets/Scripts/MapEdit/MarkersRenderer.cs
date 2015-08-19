﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarkersRenderer : MonoBehaviour {

	public		MapLuaParser			Scenario;
	public		GameObject[]			Prefabs;

	[Header("List of markers")]
	public		List<GameObject>		Armys;
	public		List<GameObject>		Mex;
	public		List<GameObject>		Hydro;
	public		List<GameObject>		Ai;	


	void LateUpdate () {
		if (Armys.Count != Scenario.ARMY_.Count || Mex.Count != Scenario.Mexes.Count || Hydro.Count != Scenario.Hydros.Count || Ai.Count != Scenario.SiMarkers.Count)
			Regenerate ();

		if (Armys.Count > 0) {
			for(int i = 0; i < Armys.Count; i++){
				Armys[i].transform.position = Scenario.ARMY_[i].position;
			}
		}

		if (Mex.Count > 0) {
			for(int i = 0; i < Mex.Count; i++){
				Mex[i].transform.position = Scenario.Mexes[i].position;
			}
		}

		if (Hydro.Count > 0) {
			for(int i = 0; i < Hydro.Count; i++){
				Hydro[i].transform.position = Scenario.Hydros[i].position;
			}
		}

		if (Ai.Count > 0) {
			for(int i = 0; i < Ai.Count; i++){
				Ai[i].transform.position = Scenario.SiMarkers[i].position;
			}
		}
	}

	public void Regenerate(){
		foreach (GameObject obj in Armys) {
			Destroy(obj);
		}
		Armys = new List<GameObject> ();
		for(int i = 0; i < Scenario.ARMY_.Count; i++){
			GameObject NewMarker = Instantiate(Prefabs[0], Scenario.ARMY_[i].position, Quaternion.identity) as GameObject;
			Armys.Add(NewMarker);
			NewMarker.transform.parent = transform;
			NewMarker.name = Scenario.ARMY_[i].name;
			NewMarker.GetComponent<MarkerData>().Rend = this;
			NewMarker.GetComponent<MarkerData>().InstanceId = i;
			NewMarker.GetComponent<MarkerData>().ListId = 0;
		}

		foreach (GameObject obj in Mex) {
			Destroy(obj);
		}
		Mex = new List<GameObject> ();
		for(int i = 0; i < Scenario.Mexes.Count; i++){
			GameObject NewMarker = Instantiate(Prefabs[1], Scenario.Mexes[i].position, Quaternion.identity) as GameObject;
			Mex.Add(NewMarker);
			NewMarker.transform.parent = transform;
			NewMarker.name = Scenario.Mexes[i].name;
			NewMarker.GetComponent<MarkerData>().Rend = this;
			NewMarker.GetComponent<MarkerData>().InstanceId = i;
			NewMarker.GetComponent<MarkerData>().ListId = 1;
		}

		foreach (GameObject obj in Hydro) {
			Destroy(obj);
		}
		Hydro = new List<GameObject> ();
		for(int i = 0; i < Scenario.Hydros.Count; i++){
			GameObject NewMarker = Instantiate(Prefabs[2], Scenario.Hydros[i].position, Quaternion.identity) as GameObject;
			Hydro.Add(NewMarker);
			NewMarker.transform.parent = transform;
			NewMarker.name = Scenario.Hydros[i].name;
			NewMarker.GetComponent<MarkerData>().Rend = this;
			NewMarker.GetComponent<MarkerData>().InstanceId = i;
			NewMarker.GetComponent<MarkerData>().ListId = 2;
		}

		foreach (GameObject obj in Ai) {
			Destroy(obj);
		}
		Ai = new List<GameObject> ();
		for(int i = 0; i < Scenario.SiMarkers.Count; i++){
			GameObject NewMarker = Instantiate(Prefabs[3], Scenario.SiMarkers[i].position, Quaternion.identity) as GameObject;
			Ai.Add(NewMarker);
			NewMarker.transform.parent = transform;
			NewMarker.name = Scenario.SiMarkers[i].name;
			NewMarker.GetComponent<MarkerData>().Rend = this;
			NewMarker.GetComponent<MarkerData>().InstanceId = i;
			NewMarker.GetComponent<MarkerData>().ListId = 3;
		}
	}
}
