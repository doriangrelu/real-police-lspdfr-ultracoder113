using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.Core
{
    class StopTimer
    {

        private const int MIN_WAIT_EVENT = 60000; // Milliseconds - Use ini file ... - 60 seconds
        private const int MAX_WAIT_EVENT = 240000; // 4 MIN

        private int MinWait = 0;
        private int MaxWait = 0;

        private static StopTimer _Instance = null;
        private Stopwatch _NextEventStopwatch = null;
        private int _NextEventTimer = 0;
        private Random _Random = null;


        /// <summary>
        /// Not a singleton because evevry evenets manager have an instance
        /// </summary>
        public StopTimer() : this(StopTimer.MIN_WAIT_EVENT, StopTimer.MAX_WAIT_EVENT)
        {
        }

        public StopTimer(int MinWait, int MaxWait)
        {
            if (MinWait >= MaxWait)
            {
                throw new Exception("Max Wait must be superior to Min Wait time...");
            }
            this.MinWait = MinWait;
            this.MaxWait = MaxWait;
            this._NextEventStopwatch = new Stopwatch();
            this._Random = new Random();
        }


        private void HandleNextWaitDuration()
        {
            this._NextEventTimer = this._Random.Next(StopTimer.MIN_WAIT_EVENT, StopTimer.MAX_WAIT_EVENT);
        }

        public void HandleTimer()
        {
            this.HandleNextWaitDuration(); 
            this._NextEventStopwatch.Reset();
            this._NextEventStopwatch.Start();
        }

        public int Timer
        {
            get
            {
                return this._NextEventTimer;
            }
        }

        public bool CanCreateEvent()
        {
            return this._NextEventStopwatch.ElapsedMilliseconds >= this._NextEventTimer;
        }


    }
}
