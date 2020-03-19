using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Net.Http;
using Proyecto26;

public class DBManager : MonoBehaviour
{
    public delegate void DBEvents1(BeaconData beacon);
    public static event DBEvents1 OnPostComplete;
    public static event DBEvents1 OnUpdateComplete;
    
    [System.Serializable]
    private class BeaconId
    {
        public string name;
    }
    private Action<List<BeaconData>> _loadAllBeaconsComplete;
    private Action _removeCallback;
    private GameObject error;

    public void SetErrorPopUp(GameObject pop)
    {
        error = pop;
    }
    
    public void UpdateBeacon(BeaconData beacon)
    {
        print("Update estimation..");
        var request = new RequestHelper
        {
            Timeout = 5,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 2,
            ContentType = "application/x-www-form-urlencoded",
            Body = beacon,
            Method = "PUT",
            Uri = "https://adminappalecso.firebaseio.com/Beacons/" + beacon.beaconId + ".json"
        };
        RestClient.Request(request).Then(response =>
            {
                OnUpdateComplete?.Invoke(beacon);
                RestClient.ClearDefaultHeaders();
            })
            .Catch(err =>
            {
                print("error on post rapport");
            });
    }
    
    public void PostNewBeacon(BeaconData beacon)
    {
        print("Post..");
        var request = new RequestHelper
        {
            Timeout = 5,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 2,
            ContentType = "application/x-www-form-urlencoded",
            Body = beacon,
            Method = "POST",
            Uri = "https://adminappalecso.firebaseio.com/Beacons.json"
        };
        RestClient.Request(request).Then(response =>
            {
                var id = JsonUtility.FromJson<BeaconId>(response.Text);
                beacon.beaconId = id.name;
                OnPostComplete?.Invoke(beacon);
                RestClient.ClearDefaultHeaders();
            })
            .Catch(err =>
            {
                print("error on post rapport");
            });
    }
    
    public void LoadBeacons(Action<List<BeaconData>> callback)
    {
        this._loadAllBeaconsComplete = callback;
        
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 3,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Beacons.json",
        };
        
        RestClient.Request(request).Then(response =>
            {
                OnGetBeaconsComplete(response.Text);
            })
            .Catch(err =>
            {
                print("error on post beacon");
                if(error != null) error.SetActive(true);
                
            }).Finally(() =>
            {
            });
    }
    
    private void OnGetBeaconsComplete(string jsonResult)
    {
        if (jsonResult == "null")
        {
            return;
        }
        print("result > " + jsonResult);
        var str = @jsonResult;
        var pList = JsonConvert.DeserializeObject<Dictionary<string, BeaconData>>(str);
        var allBeacons = new List<BeaconData>();
	    
        foreach (var p in pList)
        {
            if(!p.Value.isAvailable) continue;
                print("firebaseId : " + p.Key);
                p.Value.beaconId = p.Key; //set firebase id to our BeaconData
                allBeacons.Add(p.Value);
        }
        
        _loadAllBeaconsComplete?.Invoke(allBeacons);
    }

    public void RemoveBeacon(BeaconData beacon,Action callback)
    {
        this._removeCallback = callback;
    }
    
    public static void RemoveEvents()
    {
        OnPostComplete = null;
        OnUpdateComplete = null;
    }
}
