using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Beacon : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler
{
    public float holdingTime = 1f; //Holding time (1f = 1 sec)
    [SerializeField] private Image selectBeacon;
    public BeaconData data;

    private bool _isOnDrag;
    private float _timeMouseDown; //holding Counter
    private bool _isMouseDown; //indicate Pointer Down/up
    private bool _isHold; //indicate is Hold Click or just simple click

    public string localImagePath;
    public string localSoundPath;
    public string soundMeta;
    public string imageMeta;
    public void Initialize(BeaconData d)
    {
        print("Initialize beacon");
        this.data = d;
        _isOnDrag = false;
        _timeMouseDown = 0;
        _isMouseDown = false;
        SelectBeaconVisibility(false);
        GetComponent<RectTransform>().anchoredPosition = this.data.GetPosition();
    }

    public void OnRemoveComplete()
    {
        print("beacon removed");
        GameObject.Destroy(this.gameObject);
    }

    //-Drag & Drop Beacons
    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        _isOnDrag = true;
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(pos.x,pos.y,0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isOnDrag = false;
        this.data.SetPosition(GetComponent<RectTransform>().anchoredPosition);
    }

    //- Hold Click & Context menu
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _isMouseDown = true;
        _isHold = false;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        _isMouseDown = false;
        _timeMouseDown = 0;
    }

    //------------------- Simple click here
    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_isHold)
        {
            OnHoldClick();
            _isHold = false;
        }
    }

    
    private void OnHoldClick()
    {
        //Do something here
        BeaconMenu.Instance.AddListeners(this.OnSaveBeacon,this.OnEditBeacon,this.OnRemoveBeacon);
        BeaconMenu.Instance.OpenMenu(this);
    }

    public void SelectBeaconVisibility(bool status)
    {
        selectBeacon.enabled = status;
    }
	
    void Update()
    {
        if (_isMouseDown && !_isOnDrag)
        {
            _timeMouseDown += Time.deltaTime;
            if (_timeMouseDown >= holdingTime)
            {
                _isHold = true;
                OnHoldClick();
                _timeMouseDown = 0;
            }
        }
        else return;
    }
    
    //--------- SAVE / EDIT AND REMOVE
    private void OnSaveBeacon()
    {
        DBManager.OnUpdateComplete += OnSaveBeaconComplete;
        AdminManager.Instance.dbManager.UpdateBeacon(this.data);
    }

    private void OnSaveBeaconComplete(BeaconData beaconData)
    {
        print("Update Complete");
        DBManager.OnUpdateComplete -= OnSaveBeaconComplete;
    }
    
    private bool _isNewPicture;
    private bool _isNewSound;
    
    private void OnEditBeacon(BeaconData newData,bool isNewImg,bool isNewSound)
    {
        print("edit");
        _isNewPicture = isNewImg;
        _isNewSound = isNewSound;
        this.data = new BeaconData(newData);
       
        DBManager.OnUpdateComplete += OnSaveEditBeaconComplete;
        AdminManager.Instance.dbManager.UpdateBeacon(this.data);
    }

    private void OnSaveEditBeaconComplete(BeaconData beacon)
    {
        print("Update DB Beacon Complete");
        
        DBManager.OnUpdateComplete -= OnSaveEditBeaconComplete;
        if (_isNewPicture)
        {
            _isNewPicture = false;
            StorageHandler2.RemoveEventListener();
            StorageHandler2.OnUploadComplete += UpdatePictureComplete;
            AdminManager.Instance.storageHandler2.UploadFile(this.localImagePath,this.data.picturePath,this.imageMeta);
        }
        else
        {
            UpdatePictureComplete();
        }
    }

    private void UpdatePictureComplete()
    {
        print("UpdatePictureComplete");
        StorageHandler2.RemoveEventListener();
        StorageHandler2.OnUploadComplete -= UpdatePictureComplete;
        if (_isNewSound)
        {
            _isNewSound = false;
            StorageHandler2.OnUploadComplete += UpdateSoundComplete;
            AdminManager.Instance.storageHandler2.UploadFile(this.localSoundPath,this.data.soundPath,this.soundMeta);
        }
        else
        {
            UpdateSoundComplete();
        }
    }

    private void UpdateSoundComplete()
    {
        StorageHandler2.RemoveEventListener();
        StorageHandler2.OnUploadComplete -= UpdateSoundComplete;
        print("Update Beacon Complete");
    }

    private void OnRemoveBeacon()
    {
       // 
       GetComponent<RectTransform>().anchoredPosition = new Vector2(5000,5000);
       this.data.isAvailable = false;
       DBManager.OnUpdateComplete += OnRemoveDBBeaconComplete;
       AdminManager.Instance.dbManager.UpdateBeacon(this.data);
    }
    
    private void OnRemoveDBBeaconComplete(BeaconData beacon)
    {
        DBManager.OnUpdateComplete -= OnRemoveDBBeaconComplete;
        StorageHandler2.OnDeleteComplete += OnDeleteSoundComplete;
        AdminManager.Instance.storageHandler2.DeleteFile(this.data.soundPath);
    }
    private void OnDeleteSoundComplete()
    {
        StorageHandler2.OnDeleteComplete -= OnDeleteSoundComplete;
        StorageHandler2.OnDeleteComplete += OnDeleteImageComplete;
        AdminManager.Instance.storageHandler2.DeleteFile(this.data.picturePath);
    }

    private void OnDeleteImageComplete()
    {
        StorageHandler2.OnDeleteComplete -= OnDeleteImageComplete;
        AdminManager.Instance.RemoveBeacon(this);
        print("delete Complete");
    }


    //--- SAVE AND UPLOAD TO DATABASE
    public void SaveBeacon()
    {
        
    }
}

[System.Serializable]
public class BeaconData
{
    public string beaconId;// id on the realtime Database
    public string beaconName;
    public string title;
    public string uuid;
    public string picturePath;// firebase storage destination
    public string soundPath; // firebase storage destination
    public float xPos;
    public float yPos;
    public bool isAvailable;

    public void SetPosition(Vector2 pos)
    {
        this.xPos = pos.x;
        this.yPos = pos.y;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(this.xPos,this.yPos);
    }
    
    public BeaconData()
    {
        
    }
    public BeaconData(string id,string name,string title,string uid,string imgPath,string sound,float x,float y)
    {
        this.beaconId = id;
        this.beaconName = name;
        this.uuid = uid;
        this.title = title;
        this.soundPath = sound;
        this.picturePath = imgPath;
        this.xPos = x;
        this.yPos = y;
        this.isAvailable = true;
    }

    public BeaconData(BeaconData data)
    {
        this.beaconId = data.beaconId;
        this.beaconName = data.beaconName;
        this.uuid = data.uuid;
        this.title = data.title;
        this.soundPath = data.soundPath;
        this.picturePath = data.picturePath;
        this.isAvailable = data.isAvailable;
        this.xPos = data.xPos;
        this.yPos = data.yPos;
    }
}