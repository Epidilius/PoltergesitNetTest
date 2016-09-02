using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace hebertsystems.AVK
{
	//  Simple script to control a circular timer UI and will fire an event
	//  when time is up.
	//
	public class CircularTimer : Timer 
	{
		public override event Action timerComplete;				// Timer complete event.

		public Image timerCircle;								// The Image to update for each second (fillAmount is updated and is assumed to be radial).
		public Text timeText;									// The Text to display countdown time.

		private float currentTime;
		private bool eventSent = false;

		// Method to start the timer.
		public override void StartTimer(float time)
		{
			// Reset time and event sent flag.
			currentTime = time;
			eventSent = false;
		}

		void Update()
		{
			// Count down time.
			currentTime -= Time.deltaTime;

			// If time is up, send out event if not already sent.
			if(currentTime <= 0 && !eventSent)
			{
				currentTime = 0;

				if(timerComplete != null) timerComplete();
				eventSent = true;
			}

			// Update countdown time UI.
			if(timeText) timeText.text = currentTime <= 0 ? "0" : ((int)currentTime + 1).ToString();
			if(timerCircle) timerCircle.fillAmount = currentTime % 1;
		}
	}
}
