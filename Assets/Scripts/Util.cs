using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;

public static class Util
{
	public static int GetIndexFromName (string gameObjectName)
	{
		string prefix = string.Empty;
		int index = -1;

		if (gameObjectName.Contains ("Tile")) {
			prefix = "Tile ";	
		} else if (gameObjectName.Contains ("Token")) {
			prefix = "Token ";
		}

		int.TryParse (gameObjectName.Replace (prefix, ""), out index);

		return index;
	}
}

public class Logger
{
	private string logsFilePath;
	private bool runningInUnity;

	public Logger (string path)
	{
		runningInUnity = false;
		this.logsFilePath = path;
		var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies (); 
		foreach (Assembly assembly in loadedAssemblies) {
			if (assembly.ManifestModule.Name == "UnityEngine.dll") {
				runningInUnity = true;
			}
		}
	}

	public void LogReadable (string logMessage)
	{
		//return;
		if (this.logsFilePath.Equals (string.Empty)) {
			Console.WriteLine (logMessage);
			return;
		}

		if (runningInUnity) {
			if (logMessage.StartsWith ("=X=")) {
				Debug.LogError (logMessage);
			} else if (logMessage.StartsWith ("=!=")) {
				Debug.LogWarning (logMessage);
			} else {
				Debug.Log (logMessage);
			}
		} else {
			Console.WriteLine (logMessage);
		}

		if (Directory.Exists ((new FileInfo (logsFilePath)).DirectoryName)) {
			try {
				using (StreamWriter w = File.AppendText (logsFilePath)) {
					w.WriteLine (logMessage);
				}
			} catch (Exception e) {
				LogReadable ("=X=\tCan't write to log " + e);
			}
		}
	}
}

