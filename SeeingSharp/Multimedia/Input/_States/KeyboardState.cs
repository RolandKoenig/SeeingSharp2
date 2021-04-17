/*
    SeeingSharp and all applications distributed together with it. 
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

namespace SeeingSharp.Multimedia.Input
{
    public class KeyboardState : InputStateBase
    {
        public static readonly KeyboardState Dummy = new KeyboardState();

        // Current key states
        private List<WinVirtualKey> _keysHit;
        private List<WinVirtualKey> _keysDown;
        private bool _focused;

        public IEnumerable<WinVirtualKey> KeysDown => _keysDown;

        public IEnumerable<WinVirtualKey> KeysHit => _keysHit;

        public bool IsConnected => _focused;

        public KeyboardStateInternals Internals
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        public KeyboardState()
        {
            _keysHit = new List<WinVirtualKey>(6);
            _keysDown = new List<WinVirtualKey>(12);

            this.Internals = new KeyboardStateInternals(this);
        }

        public bool IsKeyHit(WinVirtualKey key)
        {
            if (!_focused) { return false; }

            return _keysHit.Contains(key);
        }

        public bool IsKeyDown(WinVirtualKey key)
        {
            if (!_focused) { return false; }

            return
                _keysDown.Contains(key) ||
                _keysHit.Contains(key);
        }

        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            var targetStateCasted = (KeyboardState)targetState;

            targetStateCasted._keysHit.Clear();
            targetStateCasted._keysHit.AddRange(_keysHit);

            targetStateCasted._keysDown.Clear();
            targetStateCasted._keysDown.AddRange(_keysDown);

            targetStateCasted._focused = _focused;

            // Update local collections (move hit keys to down keys)
            _keysDown.AddRange(_keysHit);
            _keysHit.Clear();
        }

        internal void NotifyKeyDown(WinVirtualKey key)
        {
            var anyRegistered =
                _keysDown.Contains(key) ||
                _keysHit.Contains(key);
            if (anyRegistered) { return; }

            _keysHit.Add(key);
        }

        internal void NotifyKeyUp(WinVirtualKey key)
        {
            while (_keysHit.Remove(key)) { }
            while (_keysDown.Remove(key)) { }
        }

        internal void NotifyFocusLost()
        {
            _focused = false;
            _keysDown.Clear();
            _keysHit.Clear();
        }

        internal void NotifyFocusGot()
        {
            _focused = true;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class KeyboardStateInternals
        {
            private KeyboardState _host;

            internal KeyboardStateInternals(KeyboardState host)
            {
                _host = host;
            }

            public void NotifyKeyDown(WinVirtualKey key)
            {
                if (key == WinVirtualKey.None) { return; }

                _host.NotifyKeyDown(key);
            }

            public void NotifyKeyUp(WinVirtualKey key)
            {
                if (key == WinVirtualKey.None) { return; }

                _host.NotifyKeyUp(key);
            }

            public void NotifyFocusGot()
            {
                _host.NotifyFocusGot();
            }

            public void NotifyFocusLost()
            {
                _host.NotifyFocusLost();
            }
        }
    }
}
