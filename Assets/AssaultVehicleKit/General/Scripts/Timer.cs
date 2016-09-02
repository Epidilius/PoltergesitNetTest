using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace hebertsystems.AVK
{
	//  Base class for a timer.  Provides a method to start the timer
	//  and an event when the timer is done.
	//
	public abstract class Timer : MonoBehaviour 
	{
		public abstract event Action timerComplete;					// Timer complete event.
		public abstract void StartTimer(float time);				// Start the timer with specified time.
	}
}
