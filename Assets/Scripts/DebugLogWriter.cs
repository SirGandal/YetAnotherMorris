using UnityEngine;
using System.IO;

public class DebugLogWriter : TextWriter
{
	public float progress = 0.0f;

	public override void Write(string value)
	{
		if(string.IsNullOrEmpty(value) || !value.Contains("="))
		{
			return;
		} 

		int iteration = 0;

		string val = value.Split('=')[1];
		if(!string.IsNullOrEmpty(val)){
			int.TryParse(val , out iteration);
			progress = (float)((iteration + 1) * 100) / (float)4096;
		}
	}

	public override System.Text.Encoding Encoding
	{
		get { return System.Text.Encoding.UTF8; }
	}
}