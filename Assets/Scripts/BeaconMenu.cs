using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class BeaconMenu : MonoBehaviour
{
    public static BeaconMenu Instance;
    [Header("Parameters")]
    [SerializeField] private FilePicker filePicker;
    [SerializeField] private float tweenSpeed = 0.3f;
    [SerializeField] private Ease ease;
    [SerializeField] private Vector2 openPos;
    [SerializeField] private Vector2 closePos;
    
    [Header("Main Menu")]
    [SerializeField] private GameObject menu;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button editBtn;
    [SerializeField] private Button removeBtn;
    [SerializeField] private Button closeBtn;
    [Header("Edit Menu")]
    [SerializeField] private GameObject editMenu;
    [SerializeField] private RectTransform panelEditMenu;
    [SerializeField] private Button browseSoundBtn;
    [SerializeField] private Button browseImageBtn;
    [SerializeField] private Button saveEditBtn;
    [SerializeField] private Button cancelEditBtn;
    [SerializeField] private Button closeEditBtn;
    [SerializeField] private InputField uuidTxt;
    [SerializeField] private InputField titleTxt;
    [SerializeField] private InputField soundPathTxt;
    [SerializeField] private InputField imagePathTxt;

    private Beacon _currentBeacon;
    private Action _saveCallback;
    private Action<BeaconData,bool,bool> _editCallback;
    private Action _removeCallback;
    
    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        this.saveBtn.onClick.AddListener(OnSaveBeacon);
        this.editBtn.onClick.AddListener(OnEditBeacon);
        this.removeBtn.onClick.AddListener(OnRemoveBeacon);
        this.closeBtn.onClick.AddListener(CloseMenu);
        InitializeEditMenu();
        this.editMenu.SetActive(false);
        menu.SetActive(false);
    }

    private void OnRemoveBeacon()
    {
        if(_currentBeacon == null) return;
        _removeCallback?.Invoke();
        CloseMenu();
    }

    private void OnEditBeacon()
    {
        if(_currentBeacon == null) return;
        OpenEditMenu();
    }

    private void OnSaveBeacon()
    {
        if(_currentBeacon == null) return;
        _saveCallback?.Invoke();
        CloseMenu();
    }

    public void AddListeners(Action save, Action<BeaconData,bool,bool> edit, Action remove)
    {
        _saveCallback = save;
        _editCallback = edit;
        _removeCallback = remove;
    }
    
    public void OpenMenu(Beacon beacon)
    {
        this._currentBeacon = beacon;
        this._currentBeacon.SelectBeaconVisibility(true); //show select layer
        menu.SetActive(true);
    }

    private void CloseMenu()
    {
        if(_currentBeacon != null) this._currentBeacon.SelectBeaconVisibility(false); //hide select layer
        _currentBeacon = null;
        _saveCallback = null;
        _editCallback = null;
        _removeCallback = null;
        menu.SetActive(false);
    }
    
    //------ EDIT MENU
    private void InitializeEditMenu()
    {
         this.browseImageBtn.onClick.AddListener(BrowseImage);   
         this.browseSoundBtn.onClick.AddListener(BrowseSound);   
         this.saveEditBtn.onClick.AddListener(SaveEdit);   
         this.cancelEditBtn.onClick.AddListener(CloseEditMenu);   
         this.closeEditBtn.onClick.AddListener(CloseEditMenu);   
         this.editMenu.SetActive(false);
    }

    private void OpenEditMenu()
    {
        //close main menu
        closeBtn.gameObject.SetActive(false);
        menu.SetActive(false);
        //set old values
        this.uuidTxt.text = this._currentBeacon.data.uuid;
        this.titleTxt.text = this._currentBeacon.data.title;
        this.soundPathTxt.text = this._currentBeacon.localSoundPath;
        this.imagePathTxt.text = this._currentBeacon.localImagePath;
        //tween panel
        this.editMenu.SetActive(true);
        this.panelEditMenu.anchoredPosition = this.closePos;
        this.panelEditMenu.DOAnchorPos(this.openPos, this.tweenSpeed).SetEase(this.ease);
    }
    
    private void CloseEditMenu()
    {
        //tween panel
        this.panelEditMenu.anchoredPosition = this.openPos;
        this.panelEditMenu.DOAnchorPos(this.closePos, this.tweenSpeed).SetEase(this.ease).OnComplete(() =>
        {
            this.editMenu.SetActive(false);
            closeBtn.gameObject.SetActive(true);
            menu.SetActive(true);
        });
    }

    private bool _isNewSound;
    private bool _isNewImage;
    private BeaconData _editBeacon;
    private void SaveEdit()
    {
        _currentBeacon.localImagePath = this.imagePathTxt.text;
        _currentBeacon.localSoundPath = this.soundPathTxt.text;

        _isNewImage = !string.IsNullOrEmpty(_currentBeacon.localImagePath);
        _isNewSound = !string.IsNullOrEmpty(_currentBeacon.localSoundPath);

        _editBeacon = new BeaconData()
        {
            beaconId = _currentBeacon.data.beaconId,
            beaconName = _currentBeacon.data.beaconName,
            title = this.titleTxt.text,
            uuid = this.uuidTxt.text,
            xPos = _currentBeacon.data.xPos,
            yPos = _currentBeacon.data.yPos,
            isAvailable = _currentBeacon.data.isAvailable
        };
        
        if (_isNewImage)
        {
            var storageImageUrl = AdminManager.Instance.GetImageStorageInfo(_currentBeacon.data.beaconName, _currentBeacon.localSoundPath);
            _editBeacon.picturePath = storageImageUrl.storageName;
            _currentBeacon.imageMeta = storageImageUrl.meta;
        }
        else
        {
            _editBeacon.picturePath = _currentBeacon.data.picturePath;
        }
        
        if (_isNewSound)
        {
            var storageSoundUrl = AdminManager.Instance.GetSoundStorageInfo(_currentBeacon.data.beaconName, _currentBeacon.localSoundPath);
            _editBeacon.soundPath = storageSoundUrl.storageName;
            _currentBeacon.soundMeta = storageSoundUrl.meta;
        }
        else
        {
            _editBeacon.soundPath = _currentBeacon.data.soundPath;
        }
       
        _editCallback?.Invoke(_editBeacon,_isNewImage,_isNewSound);
        CloseEditMenu();
    }
    
    private void BrowseSound()
    {
        this.filePicker.SetFilters(new FileBrowser.Filter( "Sounds", ".mp3", ".wav",".ogg" ),".mp3");
        this.filePicker.BrowseFile(OnSelectSoundPath,"Select a sound");
    }

    private void OnSelectSoundPath(string path)
    {
        this.soundPathTxt.text = path;
    }
    private void BrowseImage()
    {
        this.filePicker.SetFilters(new FileBrowser.Filter( "Images", ".jpg", ".jpeg",".png" ),".png");
        this.filePicker.BrowseFile(OnSelectImagePath,"Select an image");
    }
    private void OnSelectImagePath(string path)
    {
        this.imagePathTxt.text = path;
    }
}
