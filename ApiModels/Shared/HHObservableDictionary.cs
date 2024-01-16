using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.ApiModels.Shared
{
    public class HHObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<KeyValuePair<TKey, TValue>>? ItemAdded;
        public event EventHandler<KeyValuePair<TKey, TValue>>? ItemUpdated;
        public event EventHandler<TKey>? ItemRemoved;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnItemAdded(KeyValuePair<TKey, TValue> e)
        {
            Console.WriteLine("observable dictionary on item added called");
            ItemAdded?.Invoke(this, e);
        }
        protected virtual void OnItemUpdated(KeyValuePair<TKey, TValue> e)
        {
            Console.WriteLine("observable dictionary on item updated called");
            ItemUpdated?.Invoke(this, e);
        }
        protected virtual void OnItemRemoved(TKey e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        public new TValue this[TKey key]
        {
            get => base.TryGetValue(key, out var value) ? value : default!;
            set
            {
                base[key] = value;
                OnItemUpdated(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnItemAdded(new KeyValuePair<TKey, TValue>(key,value));
        }

        public new bool TryAdd(TKey key, TValue value)
        {
            var result = base.TryAdd(key, value);
            if (result)
            {
                OnItemAdded(new KeyValuePair<TKey, TValue>(key, value));
            }
            return result;
        }

        public new bool Remove(TKey key)
        {
            var result = base.Remove(key);
            if (result)
            {
                OnItemRemoved(key);
            }
            return result;
        }

        
    }
}
