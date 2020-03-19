using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    [SerializeField] private Image mapSrc;
    [SerializeField] private Transform beaconsHolder;
    [SerializeField] private GameObject beaconPrefab;
    [SerializeField] private List<Beacon> allBeacons;

    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
    }

    public void Initialize(Sprite map)
    {
        this.mapSrc.sprite = map;
        allBeacons = new List<Beacon>();
    }

    public void GenerateBeacons(List<BeaconData> list)
    {
        foreach (var beacon in list)
        {
            var beaconGo = Instantiate(beaconPrefab,beaconsHolder);
            beaconGo.transform.localScale = Vector3.one;
            var beaconScript = beaconGo.GetComponent<Beacon>();
            beaconScript.Initialize(beacon);
            allBeacons.Add(beaconScript);
        }
    }
    
    public void AddBeacon(BeaconData beacon)
    {
        if (beacon == null)
        {
            Debug.Log("MapManager : you try to add en empty or null beacon data !!");
            return;
        }
        
        var beaconGo = Instantiate(beaconPrefab,beaconsHolder);
        beaconGo.transform.localScale = Vector3.one;
        var beaconScript = beaconGo.GetComponent<Beacon>();
        if(beaconScript == null) {print("MapManager : Beacon Script Not found on the prefab !!"); return;}
        beaconScript.Initialize(beacon);
        allBeacons.Add(beaconScript);
    }

    public void Add(BeaconData beacon)
    {
        if(beaconPrefab == null) print("beaconPrefab is null");
        if(beaconsHolder == null) print("beaconsHolder is null");
        var beaconGo = Instantiate(beaconPrefab,beaconsHolder);
        print("set scale");
        beaconGo.transform.localScale = Vector3.one;
    }
    public void RemoveBeacon(Beacon beacon)
    {
        var index = this.allBeacons.IndexOf(beacon);
        this.allBeacons.RemoveAt(index);
        beacon.OnRemoveComplete();
    }
}
