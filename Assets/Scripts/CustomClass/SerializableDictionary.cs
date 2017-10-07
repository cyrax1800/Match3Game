using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[Serializable]
public class ItemColorSprite : SerializableDictionary<ItemColor, Sprite> { }
[Serializable]
public class ItemTypeSprite : SerializableDictionary<ItemType, Sprite> { }

[Serializable, DebuggerDisplay("Count = {Count}")]
public class SerializableDictionary<K, V> : IDictionary<K, V>
{

    [SerializeField, HideInInspector] int[] _Buckets;
    [SerializeField, HideInInspector] int[] _HashCodes;
    [SerializeField, HideInInspector] int[] _Next;
    [SerializeField, HideInInspector] int _Count;
    [SerializeField, HideInInspector] int _Version;
    [SerializeField, HideInInspector] int _FreeList;
    [SerializeField, HideInInspector] int _FreeCount;
    [SerializeField, HideInInspector] K[] _Keys;
    [SerializeField, HideInInspector] V[] _Values;

    readonly IEqualityComparer<K> _Comparer;

    // Mainly for debugging purposes - to get the key-value pairs display
    public Dictionary<K, V> AsDictionary
    {
        get { return new Dictionary<K, V>(this); }
    }

    public int Count
    {
        get { return _Count - _FreeCount; }
    }

    public V this[K key, V defaulV]
    {
        get
        {
            int index = FindIndex(key);
            if (index >= 0)
                return _Values[index];
            return defaulV;
        }
    }

    public V this[K key]
    {
        get
        {
            int index = FindIndex(key);
            if (index >= 0)
                return _Values[index];
            throw new KeyNotFoundException(key.ToString());
        }

        set { Insert(key, value, false); }
    }

    public SerializableDictionary()
        : this(0, null)
    {
    }

    public SerializableDictionary(int capacity)
        : this(capacity, null)
    {
    }

    public SerializableDictionary(IEqualityComparer<K> comparer)
        : this(0, comparer)
    {
    }

    public SerializableDictionary(int capacity, IEqualityComparer<K> comparer)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("capacity");

        Initialize(capacity);

        _Comparer = (comparer ?? EqualityComparer<K>.Default);
    }

    public SerializableDictionary(IDictionary<K, V> dictionary)
        : this(dictionary, null)
    {
    }

    public SerializableDictionary(IDictionary<K, V> dictionary, IEqualityComparer<K> comparer)
        : this((dictionary != null) ? dictionary.Count : 0, comparer)
    {
        if (dictionary == null)
            throw new ArgumentNullException("dictionary");

        foreach (KeyValuePair<K, V> current in dictionary)
            Add(current.Key, current.Value);
    }

    public bool ContainsValue(V value)
    {
        if (value == null)
        {
            for (int i = 0; i < _Count; i++)
            {
                if (_HashCodes[i] >= 0 && _Values[i] == null)
                    return true;
            }
        }
        else
        {
            var defaultComparer = EqualityComparer<V>.Default;
            for (int i = 0; i < _Count; i++)
            {
                if (_HashCodes[i] >= 0 && defaultComparer.Equals(_Values[i], value))
                    return true;
            }
        }
        return false;
    }

    public bool ContainsKey(K key)
    {
        return FindIndex(key) >= 0;
    }

    public void Clear()
    {
        if (_Count <= 0)
            return;

        for (int i = 0; i < _Buckets.Length; i++)
            _Buckets[i] = -1;

        Array.Clear(_Keys, 0, _Count);
        Array.Clear(_Values, 0, _Count);
        Array.Clear(_HashCodes, 0, _Count);
        Array.Clear(_Next, 0, _Count);

        _FreeList = -1;
        _Count = 0;
        _FreeCount = 0;
        _Version++;
    }

    public void Add(K key, V value)
    {
        Insert(key, value, true);
    }

    private void Resize(int newSize, bool forceNewHashCodes)
    {
        int[] bucketsCopy = new int[newSize];
        for (int i = 0; i < bucketsCopy.Length; i++)
            bucketsCopy[i] = -1;

        var keysCopy = new K[newSize];
        var valuesCopy = new V[newSize];
        var hashCodesCopy = new int[newSize];
        var nextCopy = new int[newSize];

        Array.Copy(_Values, 0, valuesCopy, 0, _Count);
        Array.Copy(_Keys, 0, keysCopy, 0, _Count);
        Array.Copy(_HashCodes, 0, hashCodesCopy, 0, _Count);
        Array.Copy(_Next, 0, nextCopy, 0, _Count);

        if (forceNewHashCodes)
        {
            for (int i = 0; i < _Count; i++)
            {
                if (hashCodesCopy[i] != -1)
                    hashCodesCopy[i] = (_Comparer.GetHashCode(keysCopy[i]) & 2147483647);
            }
        }

        for (int i = 0; i < _Count; i++)
        {
            int index = hashCodesCopy[i] % newSize;
            nextCopy[i] = bucketsCopy[index];
            bucketsCopy[index] = i;
        }

        _Buckets = bucketsCopy;
        _Keys = keysCopy;
        _Values = valuesCopy;
        _HashCodes = hashCodesCopy;
        _Next = nextCopy;
    }

    private void Resize()
    {
        Resize(PrimeHelper.ExpandPrime(_Count), false);
    }

    public bool Remove(K key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        int hash = _Comparer.GetHashCode(key) & 2147483647;
        int index = hash % _Buckets.Length;
        int num = -1;
        for (int i = _Buckets[index]; i >= 0; i = _Next[i])
        {
            if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
            {
                if (num < 0)
                    _Buckets[index] = _Next[i];
                else
                    _Next[num] = _Next[i];

                _HashCodes[i] = -1;
                _Next[i] = _FreeList;
                _Keys[i] = default(K);
                _Values[i] = default(V);
                _FreeList = i;
                _FreeCount++;
                _Version++;
                return true;
            }
            num = i;
        }
        return false;
    }

    private void Insert(K key, V value, bool add)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        if (_Buckets == null)
            Initialize(0);

        int hash = _Comparer.GetHashCode(key) & 2147483647;
        int index = hash % _Buckets.Length;
        int num1 = 0;
        for (int i = _Buckets[index]; i >= 0; i = _Next[i])
        {
            if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
            {
                if (add)
                    throw new ArgumentException("Key already exists: " + key);

                _Values[i] = value;
                _Version++;
                return;
            }
            num1++;
        }
        int num2;
        if (_FreeCount > 0)
        {
            num2 = _FreeList;
            _FreeList = _Next[num2];
            _FreeCount--;
        }
        else
        {
            if (_Count == _Keys.Length)
            {
                Resize();
                index = hash % _Buckets.Length;
            }
            num2 = _Count;
            _Count++;
        }
        _HashCodes[num2] = hash;
        _Next[num2] = _Buckets[index];
        _Keys[num2] = key;
        _Values[num2] = value;
        _Buckets[index] = num2;
        _Version++;

        //if (num3 > 100 && HashHelpers.IsWellKnownEqualityComparer(comparer))
        //{
        //    comparer = (IEqualityComparer<K>)HashHelpers.GetRandomizedEqualityComparer(comparer);
        //    Resize(entries.Length, true);
        //}
    }

    private void Initialize(int capacity)
    {
        int prime = PrimeHelper.GetPrime(capacity);

        _Buckets = new int[prime];
        for (int i = 0; i < _Buckets.Length; i++)
            _Buckets[i] = -1;

        _Keys = new K[prime];
        _Values = new V[prime];
        _HashCodes = new int[prime];
        _Next = new int[prime];

        _FreeList = -1;
    }

    private int FindIndex(K key)
    {
        if (key == null)
            throw new ArgumentNullException("key");

        if (_Buckets != null)
        {
            int hash = _Comparer.GetHashCode(key) & 2147483647;
            for (int i = _Buckets[hash % _Buckets.Length]; i >= 0; i = _Next[i])
            {
                if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
                    return i;
            }
        }
        return -1;
    }

    public bool TryGeV(K key, out V value)
    {
        int index = FindIndex(key);
        if (index >= 0)
        {
            value = _Values[index];
            return true;
        }
        value = default(V);
        return false;
    }

    private static class PrimeHelper
    {
        public static readonly int[] Primes = new int[]
        {
            3,
            7,
            11,
            17,
            23,
            29,
            37,
            47,
            59,
            71,
            89,
            107,
            131,
            163,
            197,
            239,
            293,
            353,
            431,
            521,
            631,
            761,
            919,
            1103,
            1327,
            1597,
            1931,
            2333,
            2801,
            3371,
            4049,
            4861,
            5839,
            7013,
            8419,
            10103,
            12143,
            14591,
            17519,
            21023,
            25229,
            30293,
            36353,
            43627,
            52361,
            62851,
            75431,
            90523,
            108631,
            130363,
            156437,
            187751,
            225307,
            270371,
            324449,
            389357,
            467237,
            560689,
            672827,
            807403,
            968897,
            1162687,
            1395263,
            1674319,
            2009191,
            2411033,
            2893249,
            3471899,
            4166287,
            4999559,
            5999471,
            7199369
        };

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int num = (int)Math.Sqrt((double)candidate);
                for (int i = 3; i <= num; i += 2)
                {
                    if (candidate % i == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }

        public static int GetPrime(int min)
        {
            if (min < 0)
                throw new ArgumentException("min < 0");

            for (int i = 0; i < PrimeHelper.Primes.Length; i++)
            {
                int prime = PrimeHelper.Primes[i];
                if (prime >= min)
                    return prime;
            }
            for (int i = min | 1; i < 2147483647; i += 2)
            {
                if (PrimeHelper.IsPrime(i) && (i - 1) % 101 != 0)
                    return i;
            }
            return min;
        }

        public static int ExpandPrime(int oldSize)
        {
            int num = 2 * oldSize;
            if (num > 2146435069 && 2146435069 > oldSize)
            {
                return 2146435069;
            }
            return PrimeHelper.GetPrime(num);
        }
    }

    public ICollection<K> Keys
    {
        get { return _Keys.Take(Count).ToArray(); }
    }

    public ICollection<V> Values
    {
        get { return _Values.Take(Count).ToArray(); }
    }

    public void Add(KeyValuePair<K, V> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<K, V> item)
    {
        int index = FindIndex(item.Key);
        return index >= 0 &&
            EqualityComparer<V>.Default.Equals(_Values[index], item.Value);
    }

    public void CopyTo(KeyValuePair<K, V>[] array, int index)
    {
        if (array == null)
            throw new ArgumentNullException("array");

        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(string.Format("index = {0} array.Length = {1}", index, array.Length));

        if (array.Length - index < Count)
            throw new ArgumentException(string.Format("The number of elements in the dictionary ({0}) is greater than the available space from index to the end of the destination array {1}.", Count, array.Length));

        for (int i = 0; i < _Count; i++)
        {
            if (_HashCodes[i] >= 0)
                array[index++] = new KeyValuePair<K, V>(_Keys[i], _Values[i]);
        }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        return Remove(item.Key);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryGetValue(K key, out V value)
    {
        return ((IDictionary<K, V>)AsDictionary).TryGetValue(key, out value);
    }

    public struct Enumerator : IEnumerator<KeyValuePair<K, V>>
    {
        private readonly SerializableDictionary<K, V> _Dictionary;
        private int _Version;
        private int _Index;
        private KeyValuePair<K, V> _Current;

        public KeyValuePair<K, V> Current
        {
            get { return _Current; }
        }

        internal Enumerator(SerializableDictionary<K, V> dictionary)
        {
            _Dictionary = dictionary;
            _Version = dictionary._Version;
            _Current = default(KeyValuePair<K, V>);
            _Index = 0;
        }

        public bool MoveNext()
        {
            if (_Version != _Dictionary._Version)
                throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", _Version, _Dictionary._Version));

            while (_Index < _Dictionary._Count)
            {
                if (_Dictionary._HashCodes[_Index] >= 0)
                {
                    _Current = new KeyValuePair<K, V>(_Dictionary._Keys[_Index], _Dictionary._Values[_Index]);
                    _Index++;
                    return true;
                }
                _Index++;
            }

            _Index = _Dictionary._Count + 1;
            _Current = default(KeyValuePair<K, V>);
            return false;
        }

        void IEnumerator.Reset()
        {
            if (_Version != _Dictionary._Version)
                throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", _Version, _Dictionary._Version));

            _Index = 0;
            _Current = default(KeyValuePair<K, V>);
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }
    }
}