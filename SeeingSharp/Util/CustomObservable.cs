#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public class CustomObservable<T> : IObservable<T>
    {
        private List<IObserver<T>> m_observers;
        private bool m_isFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomObservable{T}"/> class.
        /// </summary>
        public CustomObservable()
        {
            m_observers = new List<IObserver<T>>();
            m_isFinished = false;
        }

        /// <summary>
        /// Subscribes the specified observer.
        /// </summary>
        /// <param name="observer">The observer to be subscribed.</param>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            m_observers.Add(observer);

            return new DummyDisposable(
                () => m_observers.Remove(observer));
        }

        public void PushNext(T nextObject)
        {
            for(int loop=0; loop<m_observers.Count; loop++)
            {
                m_observers[loop].OnNext(nextObject);
            }
        }

        public void PushError(Exception error)
        {
            m_isFinished.EnsureFalse(nameof(m_isFinished));

            m_isFinished = true;
            for (int loop = 0; loop < m_observers.Count; loop++)
            {
                m_observers[loop].OnError(error);
            }
        }

        public void PushCompleted()
        {
            m_isFinished.EnsureFalse(nameof(m_isFinished));

            m_isFinished = true;
            for (int loop = 0; loop < m_observers.Count; loop++)
            {
                m_observers[loop].OnCompleted();
            }
        }
    }
}
