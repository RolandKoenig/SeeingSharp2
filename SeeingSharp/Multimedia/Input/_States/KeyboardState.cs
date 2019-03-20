/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
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

using System.Collections.Generic;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Input
{
    public class KeyboardState : InputStateBase
    {
        public static readonly KeyboardState Dummy = new KeyboardState();

        // Current key states
        private List<WinVirtualKey> m_keysHit;
        private List<WinVirtualKey> m_keysDown;
        private bool m_focused;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        public KeyboardState()
        {
            m_keysHit = new List<WinVirtualKey>(6);
            m_keysDown = new List<WinVirtualKey>(12);

            this.Internals = new KeyboardStateInternals(this);
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
            var targetStateCasted = targetState as KeyboardState;
            targetStateCasted.EnsureNotNull(nameof(targetStateCasted));

            targetStateCasted.m_keysHit.Clear();
            targetStateCasted.m_keysHit.AddRange(m_keysHit);

            targetStateCasted.m_keysDown.Clear();
            targetStateCasted.m_keysDown.AddRange(m_keysDown);

            targetStateCasted.m_focused = m_focused;

            // Update local collections (move hit keys to down keys)
            m_keysDown.AddRange(m_keysHit);
            m_keysHit.Clear();
        }

        internal void NotifyKeyDown(WinVirtualKey key)
        {
            var anyRegistered =
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

        public IEnumerable<WinVirtualKey> KeysDown => m_keysDown;

        public IEnumerable<WinVirtualKey> KeysHit => m_keysHit;

        public bool IsConnected => m_focused;

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
