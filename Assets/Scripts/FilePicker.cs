﻿using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.UI;

public class FilePicker : MonoBehaviour
{
	private Action<string> _callback;
	private string _title;
	public void SetFilters(FileBrowser.Filter filter,string defaultFilter)
	{
		
		//FileBrowser.SetFilters( true, new FileBrowser.Filter( "Sounds", ".mp3", ".wav",".ogg" ) );
		FileBrowser.SetFilters( true, filter);
		//FileBrowser.SetDefaultFilter( ".mp3" );
		FileBrowser.SetDefaultFilter( defaultFilter);
		FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" );
	}
   
    public void BrowseFile(Action<string> callback,string title)
    {
	    this._title = title;
	    this._callback = callback;
	    StartCoroutine( ShowLoadDialogCoroutine() );
    }
    
    private IEnumerator ShowLoadDialogCoroutine()
    {
	    // Show a load file dialog and wait for a response from user
	    // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
	    yield return FileBrowser.WaitForLoadDialog( false, null, this._title, "Select" );

	    // Dialog is closed
	    // Print whether a file is chosen (FileBrowser.Success)
	    // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
	    Debug.Log( FileBrowser.Success + " " + FileBrowser.Result );
	    if (FileBrowser.Result != null)
	    {
		    this._callback?.Invoke(FileBrowser.Result);
		    this._callback = null;
	    }
    }

}
