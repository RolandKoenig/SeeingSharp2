#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Input
{
    public class KeyboardState : InputStateBase
    {
        public static readonly KeyboardState Dummy = new KeyboardState();

        #region current key states
        private List<WinVirtualKey> m_keysHit;
        private List<WinVirtualKey> m_keysDown;
        private bool m_focused;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        public KeyboardState()
        {
            m_keysHit = new List<WinVirtualKey>(6);
            m_keysDown = new List<WinVirtualKey>(12);

            this.Internals = new KeyboardStateInternals(this);
        }

        internal void NotifyKeyDown(WinVirtualKey key)
        {
            bool anyRegistered =
                m_keysDown.Contains(key) ||
                m_keysHit.Contains(key);
            if (anyRegistered) { return; }

            m_keysHit.Add(key);
        }

        internal void NotifyKeyUp(WinVirtualKey key)
        {
            while (m_keysHit.Remove(key)) { }
            while (m_keysDown.Remove(key)) { }
        }

        internal void NotifyFocusLost()
        {
            m_focused = false;
            m_keysDown.Clear();
            m_keysHit.Clear();
        }

        internal void NotifyFocusGot()
        {
            m_focused = true;
        }

        public bool IsKeyHit(WinVirtualKey key)
        {
            if (!m_focused) { return false; }

            return m_keysHit.Contains(key);
        }

        public bool IsKeyDown(WinVirtualKey key)
        {
            if (!m_focused) { return false; }

            return
                m_keysDown.Contains(key) ||
                m_keysHit.Contains(key);
        }

        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            KeyboardState targetStateCasted = targetState as KeyboardState;
            targetStateCasted.EnsureNotNull(nameof(targetStateCasted));

            targetStateCasted.m_keysHit.Clear();
            targetStateCasted.m_keysHit.AddRange(this.m_keysHit);

            targetStateCasted.m_keysDown.Clear();
            targetStateCasted.m_keysDown.AddRange(this.m_keysDown);

            targetStateCasted.m_focused = this.m_focused;

            // Update local collections (move hit keys to down keys)
            this.m_keysDown.AddRange(this.m_keysHit);
            this.m_keysHit.Clear();
        }

        public IEnumerable<WinVirtualKey> KeysDown
        {
            get { return m_keysDown; }
        }

        public IEnumerable<WinVirtualKey> KeysHit
        {
            get { return m_keysHit; }
        }

        public bool IsConnected
        {
            get { return m_focused; }
        }

        public KeyboardStateInternals Internals
        {
            get;
            private set;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class KeyboardStateInternals
        {
            private KeyboardState m_host;

            internal KeyboardStateInternals(KeyboardState host)
            {
                m_host = host;
            }

            public void NotifyKeyDown(WinVirtualKey key)
            {
                m_host.NotifyKeyDown(key);
            }

            public void NotifyKeyUp(WinVirtualKey key)
            {
                m_host.NotifyKeyUp(key);
            }

            public void NotifyFocusGot()
            {
                m_host.NotifyFocusGot();
            }

            public void NotifyFocusLost()
            {
                m_host.NotifyFocusLost();
            }
        }
    }
}
