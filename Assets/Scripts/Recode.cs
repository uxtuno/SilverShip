using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class Recode : MonoBehaviour
{
	//A boolean that flags whether there's a connected microphone  
	//private bool micConnected = false;

	//The maximum and minimum available recording frequencies  
	private int minFreq;
	private int maxFreq;

	//A handle to the attached AudioSource  
	private AudioSource goAudioSource;

	//Use this for initialization  
	void Start()
	{
		//Check if there is at least one microphone connected  
		if (Microphone.devices.Length <= 0)
		{
			//Throw a warning message at the console if there isn't  
			//UnityEngine.Debug.LogWarning("Microphone not connected!");
			enabled = false;
		}
		else //At least one microphone is present  
		{
			//Set 'micConnected' to true  
			//micConnected = true;

			//Get the default microphone recording capabilities  
			Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

			//According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  
			if (minFreq == 0 && maxFreq == 0)
			{
				//...meaning 44100 Hz can be used as the recording sampling rate  
				maxFreq = 44100;
			}

			//Get the attached AudioSource component  
			goAudioSource = this.GetSafeComponent<AudioSource>();
		}
	}

	[Conditional("DEBUG")]
	void Update()
	{
		if (!Microphone.IsRecording(null))
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				goAudioSource.clip = Microphone.Start(null, true, 20, maxFreq);
				print("録音開始");
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Microphone.End(null); //Stop the audio recording  
				goAudioSource.Play(); //Playback the recorded audio  
				print("録音終了 ＆ 再生");
			}
		}
	}
}
