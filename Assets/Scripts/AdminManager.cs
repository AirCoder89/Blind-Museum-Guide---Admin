using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class AdminManager : MonoBehaviour
{
    public static AdminManager Instance;
    [SerializeField] private Sprite map;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject errorLoading;
    public MapManager mapManager;
    [SerializeField] private AddNewBeaconPopUp addBeaconPopUp;
    public DBManager dbManager;
    public StorageHandler storageHandler;
    public StorageHandler2 storageHandler2;
    [SerializeField] private Button addNewBeaconBtn;

    private Action _removeCallback;
    
    private void Awake()
    {
        if(Instance != null )return;
        Instance = this;
    }

    private void Start()
    {
        this.addBeaconPopUp.Initialize();
        this.mapManager.Initialize(this.map);
        dbManager.SetErrorPopUp(this.errorLoading);
        this.addNewBeaconBtn.onClick.AddListener(OnAddNewBeacon);
        LoadBeacons();
    }

    public void Reload()
    {
        LoadBeacons();
    }
    private void LoadBeacons()
    {
        loadingPanel.SetActive(true);
        errorLoading.SetActive(false);
        dbManager.LoadBeacons(OnLoadBeaconsComplete);
    }

    private void OnLoadBeaconsComplete(List<BeaconData> beacons)
    {
        this.mapManager.GenerateBeacons(beacons);
        loadingPanel.SetActive(false);
    }

    private void OnAddNewBeacon()
    {
        this.addBeaconPopUp.Open();
    }

    public void RemoveBeacon(Beacon beacon)
    {
        this.mapManager.RemoveBeacon(beacon);
    }

    private void OnRemoveBeaconComplete()
    {
        this._removeCallback?.Invoke();
    }

    
    public (string storageName, string meta) GetSoundStorageInfo(string beaconName, string localPath)
    {
        var extension = localPath.Split(char.Parse("."))[1];
        return (beaconName + "_sound." + extension, "audio/" + extension);
    }
    public (string storageName, string meta) GetImageStorageInfo(string beaconName, string localPath)
    {
        var extension = localPath.Split(char.Parse("."))[1];
        return (beaconName + "_image." + extension, "image/" + extension);
    }
    
    public string GetStorageSoundUrl(string beaconName,string localPath)
    {
        //----
        var extension = "." + localPath.Split(char.Parse("."))[1];
        //-----
        return this.storageHandler.MyStorageBucket + beaconName + "_sound" + extension;
    }
    
    public string GetStorageImageUrl(string beaconName,string localPath)
    {
        //----
        var extension = "." + localPath.Split(char.Parse("."))[1];
        //-----
        return this.storageHandler.MyStorageImageBucket + beaconName + "_image" + extension;
    }
    
    public string RandomString(int size, bool lowerCase)  
    {  
        var builder = new StringBuilder();  
        var random = new Random();  
        char ch;  
        for (var i = 0; i < size; i++)  
        {  
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));  
            builder.Append(ch);  
        }  
        if (lowerCase)  
            return builder.ToString().ToLower();  
        return builder.ToString();  
    }  
}
