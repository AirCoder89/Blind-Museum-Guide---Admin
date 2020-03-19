using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class AddNewBeaconPopUp : MonoBehaviour
{
    [SerializeField] private Button submitBtn;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private Button browseSoundBtn;
    [SerializeField] private Button browseImageBtn;
    [SerializeField] private Button finishBtn;
    [SerializeField] private InputField uuidTxt;
    [SerializeField] private InputField soundPathTxt;
    [SerializeField] private InputField imagePathTxt;
    [SerializeField] private InputField beaconTitleTxt;
    [SerializeField] private FilePicker filePicker;
    
    private RectTransform _rt;
    private (string storageName,string meta) _storageSoundInfo;
    private (string storageName,string meta) _storageImageInfo;
    private string _beaconName;

    private BeaconData _postedBeacon;
    public void Initialize()
    {
        submitBtn.onClick.AddListener(OnSubmit);
        cancelBtn.onClick.AddListener(Close);
        finishBtn.onClick.AddListener(GenerateBeacon);
        browseSoundBtn.onClick.AddListener(OnBrowseSound);
        browseImageBtn.onClick.AddListener(OnBrowseImage);
        
        finishBtn.gameObject.SetActive(true);
        submitBtn.gameObject.SetActive(true);
        submitBtn.interactable = true;
        cancelBtn.gameObject.SetActive(true);
        cancelBtn.interactable = true;
        Close();
    }

    private void OnBrowseImage()
    {
        this.filePicker.SetFilters(new FileBrowser.Filter( "Images", ".jpg", ".jpeg",".png" ),".png");
        this.filePicker.BrowseFile(OnSelectImagePath,"Select an image");
    }
    private void OnSelectImagePath(string path)
    {
        imagePathTxt.text = path;
    }
    private void OnBrowseSound()
    {
        this.filePicker.SetFilters(new FileBrowser.Filter( "Sounds", ".mp3", ".wav",".ogg" ),".mp3");
        this.filePicker.BrowseFile(OnSelectSoundPath,"Select a sound");
    }

    private void OnSelectSoundPath(string path)
    {
        soundPathTxt.text = path;
    }
    
    private void OnSubmit()
    {
        if (string.IsNullOrEmpty(soundPathTxt.text) || string.IsNullOrEmpty(this.uuidTxt.text) || string.IsNullOrEmpty(this.imagePathTxt.text))
        {
            return;
        }
        
        cancelBtn.interactable = false;
        submitBtn.interactable = false;
        
        _beaconName = AdminManager.Instance.RandomString(16, true);
        _storageSoundInfo = AdminManager.Instance.GetSoundStorageInfo(_beaconName, soundPathTxt.text);
        _storageImageInfo = AdminManager.Instance.GetImageStorageInfo(_beaconName, imagePathTxt.text);
       
        var beacon = new BeaconData()
        {
            beaconName = this._beaconName,
            soundPath = _storageSoundInfo.storageName,
            picturePath = _storageImageInfo.storageName,
            title = this.beaconTitleTxt.text,
            xPos = 0f,
            yPos = 0,
            uuid = this.uuidTxt.text,
            isAvailable = true
        };
        DBManager.OnPostComplete += PostBeaconComplete;
        AdminManager.Instance.dbManager.PostNewBeacon(beacon);
    }
    
    private void PostBeaconComplete(BeaconData beacon)
    {
        print("Post beacon on db complete : " + beacon.beaconId);
        DBManager.OnPostComplete -= PostBeaconComplete;
        this._postedBeacon = beacon;
        
        StorageHandler2.RemoveEventListener();
        StorageHandler2.OnUploadComplete += UploadSoundComplete;
        AdminManager.Instance.storageHandler2.UploadFile(soundPathTxt.text,_storageSoundInfo.storageName,_storageSoundInfo.meta);
    }

    private void UploadSoundComplete()
    {
        print("Upload sound complete");
        StorageHandler2.RemoveEventListener();
        StorageHandler2.OnUploadComplete -= UploadSoundComplete;
        StorageHandler2.OnUploadComplete += UploadImageComplete;
        AdminManager.Instance.storageHandler2.UploadFile(imagePathTxt.text,_storageImageInfo.storageName,_storageImageInfo.meta);
    }

    private void UploadImageComplete()
    {
        StorageHandler2.RemoveEventListener();
        StorageHandler2.OnUploadComplete -= UploadImageComplete;
        print("ADD BEACON COMPLETE");
        finishBtn.gameObject.SetActive(true);
        submitBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(false);
    }

    public void GenerateBeacon()
    {
        MapManager.Instance.AddBeacon(this._postedBeacon);
        Close();
    }
    
    private void UploadError()
    {
        print("Upload error");
    }
    
    private void PostError(string error)
    {
        print("Upload error");
    }
    
    public void Open()
    {
        finishBtn.gameObject.SetActive(true);
        submitBtn.gameObject.SetActive(true);
        submitBtn.interactable = true;
        cancelBtn.gameObject.SetActive(true);
        cancelBtn.interactable = true;
        gameObject.SetActive(true);
    }

    private void Close()
    {
        finishBtn.gameObject.SetActive(true);
        submitBtn.gameObject.SetActive(true);
        submitBtn.interactable = true;
        cancelBtn.gameObject.SetActive(true);
        cancelBtn.interactable = true;
        
        imagePathTxt.text = "";
        soundPathTxt.text = "";
        beaconTitleTxt.text = "";
        uuidTxt.text = "";
        StorageHandler2.RemoveEventListener();
        gameObject.SetActive(false);
    }
    
}
