using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {
    public struct Observable<T> : IEquatable<T> {
        private T value;
        public Action<T> OnChanged;
        public T Value {
            get { return value; }
            set {
                if (Equals(value)) return;
                this.value = value;
                NotifyPropertyChanged();
            }
        }

        public Observable(T value) { this.value = value; OnChanged = null; }

        public bool Equals(T other) {
            if (value == null) {
                if (other == null) return true;
            } else {
                if (other != null) return value.Equals(other);
            }
            return false;
        }

        public void NotifyPropertyChanged() {
            if (OnChanged != null) OnChanged(Value);
        }

        public static bool operator ==(Observable<T> o, T other) { return o.Equals(other); }
        public static bool operator !=(Observable<T> o, T other) { return !o.Equals(other); }
        public static implicit operator T(Observable<T> o) { return o.Value; }

    
    }
}
