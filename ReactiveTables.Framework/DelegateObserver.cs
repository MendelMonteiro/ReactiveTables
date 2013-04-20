/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace ReactiveTables.Framework
{
    public class DelegateObserver<T> : IObserver<T>
    {
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext)
        {
            return new DelegateObserver<T>(onNext, null, null);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action onCompleted)
        {
            return new DelegateObserver<T>(onNext, null, onCompleted);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action<Exception> onError)
        {
            return new DelegateObserver<T>(onNext, onError, null);
        }
        public static DelegateObserver<T> CreateDelegateObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return new DelegateObserver<T>(onNext, onError, onCompleted);
        }

        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;


        public DelegateObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            if (_onError != null)
            {
                
                _onError(error);
            }
        }

        public void OnCompleted()
        {
            if (_onCompleted != null)
            {
                _onCompleted();
            }
        }
    }
}
