﻿using CommandEngine;

namespace CommandEngine
{
    public class Timer
    {
        public bool hasFired { get; private set; }
        public bool isActive { get; private set; }
        public bool canActivate { get; private set; }

        public double timeStart;
        public double timeLeft { get; private set; }
        public double timePassed { get { return timeStart - timeLeft; } }
        public double percentComplete { get { return timePassed / timeStart; } }

        public Timer(double time, bool active = false, bool canActivate = true)
        {
            hasFired = false;
            isActive = false;
            this.canActivate = canActivate;
            timeLeft = timeStart = time;

            if (active)
                Activate();
        }

        /// <summary> Updates the timer by deltaTime. </summary>
        /// <param name="modTime"> Additional time to take off the timer. Negative numbers increase the timer. </param>
        public bool Update(double dt)
        {
            if (isActive)
            {
                timeLeft -= dt;
                if (timeLeft <= 0.0f)
                {
                    hasFired = true;
                    isActive = false;
                    timeLeft = 0.0f;
                    return true;
                }
            }
            return false;
        }

        /// <summary> Updates the timer by deltaTime if condition is true. </summary>
        public bool Update(double dt, bool condition)
        {
            if (condition)
                return Update(dt);
            return false;
        }

        /// <summary> Start the timer. Returns false if timer cannot activate. </summary>
        public bool Activate()
        {
            if (canActivate)
            {
                hasFired = false;
                isActive = true;
                timeLeft = timeStart;
            }
            return canActivate;
        }

        /// <summary> Start the timer. Returns false if timer cannot activate. </summary>
        /// <param name="time"> Changes the start time. </param>
        public bool Activate(double time)
        {
            timeStart = time;
            return Activate();
        }

        /// <summary>  Stop the timer. </summary>
        /// <param name="forceToFire"> If true, the timer sets its state as if it fired normally. </param>
        public void Deactivate(bool forceToFire = false)
        {
            isActive = false;
            hasFired = forceToFire;
        }

        public void SetPercentComplete(double percent)
        {
            if (!isActive) return;
            percent = Mathc.Clamp(percent, 0.0f, 1.0f);
            if (percent == 1.0f)
            {
                hasFired = true;
                isActive = false;
                timeLeft = 0.0f;
            }
            else
            {
                timeLeft = (1.0f - percent) * timeStart;
            }
        }

        public void AllowActivation(bool val)
        {
            canActivate = val;
        }
    }

    public class TimeCounter
    {
        public double currentTime { get; private set; }
        public double maxTimeAllowed;

        public TimeCounter(double maxTimeAllowed)
        {
            this.maxTimeAllowed = maxTimeAllowed <= 0.0f ? double.MaxValue : maxTimeAllowed;
        }

        void Update(double time)
        {
            currentTime = Mathc.Clamp(currentTime + time, 0.0f, maxTimeAllowed);
        }

        public void AddTime(double time)
        {
            Update(time);
        }

        public void SubTime(double time)
        {
            Update(-time);
        }

        public void Reset()
        {
            currentTime = 0.0f;
        }
    }

    public class FrameTimer
    {
        public bool hasFired { get; private set; }
        public bool isActive { get; private set; }
        public bool canActivate { get; private set; }
        public int timerStart;
        public int timeLeft { get; private set; }
        public int timePassed { get { return timerStart - timeLeft; } }
        public double percentComplete { get { return (double)timePassed / (double)timerStart; } }

        public FrameTimer(int time, bool active = false, bool canActivate = true)
        {
            hasFired = false;
            isActive = false;
            this.canActivate = canActivate;
            timeLeft = timerStart = time;

            if (active)
                Activate();
        }

        /// <summary> Updates the timer by one frame. </summary>
        public bool Update()
        {
            return Update(1);
        }

        /// <summary> Updates the timer by numFrames. </summary>
        public bool Update(int numFrames)
        {
            if (isActive)
            {
                timeLeft -= numFrames;
                if (timeLeft <= 0)
                {
                    hasFired = true;
                    isActive = false;
                    timeLeft = 0;
                    return true;
                }
            }
            return false;
        }

        /// <summary> Updates the timer by deltaTime if condition is true. </summary>
        public bool Update(int numFrames, bool condition)
        {
            if (condition)
                return Update(numFrames);
            return false;
        }

        /// <summary> Start the timer. Returns false if timer cannot activate. </summary>
        /// <param name="time"> Changes the start time. </param>
        public bool Activate(int time)
        {
            timerStart = time;
            return Activate();
        }

        /// <summary> Start the timer. Returns false if timer cannot activate. </summary>
        public bool Activate()
        {
            if (canActivate)
            {
                hasFired = false;
                isActive = true;
                timeLeft = timerStart;
            }
            return canActivate;
        }

        /// <summary>  Stop the timer. </summary>
        /// <param name="forceToFire"> Force the timer to fire, instead of just stopping. </param>
        public void Deactivate(bool forceToFire = false)
        {
            isActive = false;
            hasFired = forceToFire;
        }

        public void SetPercentComplete(double percent)
        {
            if (!isActive) return;
            percent = Mathc.Clamp(percent, 0.0f, 1.0f);
            if (percent == 1.0f)
            {
                hasFired = true;
                isActive = false;
                timeLeft = 0;
            }
            else
            {
                timeLeft = (int)((1.0f - percent) * timerStart);
            }
        }

        public void AllowActivation(bool val)
        {
            canActivate = val;
        }
    } 
}