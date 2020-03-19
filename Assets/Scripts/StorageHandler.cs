
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

public class StorageHandler : MonoBehaviour
{
   public delegate void StorageEvents(Firebase.Storage.StorageMetadata result);
   public static event StorageEvents OnUploadComplete;
   
   public Text logTxt;
   public string localFilePath;
   public string storageFilePath;
   
   public string MyStorageBucket = "gs://adminappalecso.appspot.com/Sounds/";
   public string MyStorageImageBucket = "gs://adminappalecso.appspot.com/Images/";
   
   private FirebaseStorage storage;
   private bool operationInProgress;
   private bool isFirebaseInitialized = false;
   private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
   private string UriFileScheme = Uri.UriSchemeFile + "://";

   private string rootStorageUrl = "gs://adminappalecso.appspot.com/";
   protected virtual void Start() {
      FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
         dependencyStatus = task.Result;
         if (dependencyStatus == DependencyStatus.Available) {
            InitializeFirebase();
         } else {
            Debug.LogError(
               "Could not resolve all Firebase dependencies: " + dependencyStatus);
         }
      });
   }

   protected virtual void InitializeFirebase() {
      var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
      storage = FirebaseStorage.DefaultInstance;
      
      if (!String.IsNullOrEmpty(appBucket)) {
         rootStorageUrl = String.Format("gs://{0}/", appBucket);
      }
      isFirebaseInitialized = true;
   }
   
   public void DebugLog(string s) {
      Debug.Log(s);
      logTxt.text += s + "\n";
   }
   
   // Retrieve a storage reference from the user specified path.
   private StorageReference GetStorageReference() {
      // If this is an absolute path including a bucket create a storage instance.
      if (storageFilePath.StartsWith("gs://") ||
          storageFilePath.StartsWith("http://") ||
          storageFilePath.StartsWith("https://")) {
         var storageUri = new Uri(storageFilePath);
         var firebaseStorage = FirebaseStorage.GetInstance(
            String.Format("{0}://{1}", storageUri.Scheme, storageUri.Host));
         return firebaseStorage.GetReferenceFromUrl(storageFilePath);
      }
      // When using relative paths use the default storage instance which uses the bucket supplied
      // on creation of FirebaseApp.
      return FirebaseStorage.DefaultInstance.GetReference(storageFilePath);
   }

   public static void RemoveEvents()
   {
      OnUploadComplete = null;
   }
   
   public void UploadFileToStorage(string from,string to)
   {
      localFilePath = from;
      storageFilePath = to;
      
      print($"Upload from {localFilePath} to {storageFilePath}");
      UploadFile();
   }
   
   // Get the local filename as a URI relative to the persistent data path if the path isn't
   // already a file URI.
   protected virtual string PathToPersistentDataPathUriString(string filename) {
      if (filename.StartsWith(UriFileScheme)) {
         return filename;
      }
      return String.Format("{0}{1}", UriFileScheme,filename);
   }
  
   private void UploadFile()
   {
      // File located on disk
      print("up");
      string local_file = PathToPersistentDataPathUriString(localFilePath);
      print("Upload from : " + local_file);
      var storageReference = GetStorageReference();

         storageReference.PutFileAsync(local_file)
            .ContinueWith ((Task<StorageMetadata> task) => {
               if (task.IsFaulted || task.IsCanceled) {
                  DebugLog(task.Exception.ToString());
                  // Uh-oh, an error occurred!
               } else {
                  // Metadata contains file metadata such as size, content-type, and download URL.
                  Firebase.Storage.StorageMetadata metadata = task.Result;
                  OnUploadComplete?.Invoke(metadata);
                  DebugLog("Finished uploading...");
               }
            });
   }

   public void DownloadFileFromStorage(string from,string to,Action callback,Action errorCb)
   {
      localFilePath = to;
      storageFilePath = from;
      StartCoroutine(DownloadFile());
   }
   
   private IEnumerator DownloadFile()
   {
      string local_file = PathToPersistentDataPathUriString(localFilePath);
      print("Download To : " + local_file);
      var storageReference = GetStorageReference();
      // Download to the local filesystem
      storageReference.GetFileAsync(local_file).ContinueWith(task => {
         if (!task.IsFaulted && !task.IsCanceled) {
            DebugLog("File downloaded.");
         }
         else
         {
            
         }
      });
     
      yield return null;
   }

   public void DeleteFileFromStorage(string sUrl, Action callback,Action errorCb)
   {
      storageFilePath = sUrl;
      StartCoroutine(DeleteFile());
   }
   
   private IEnumerator DeleteFile()
   {
      var storageReference = GetStorageReference();
      // Delete the file
      storageReference.DeleteAsync().ContinueWith(task => {
         if (task.IsCompleted) {
            DebugLog("File deleted successfully.");
         } else {
            // Uh-oh, an error occurred!
            DebugLog("an error occurred");
         }
      });
    
      yield return null;
   }
   
}
