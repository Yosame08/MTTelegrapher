using System;

public class CircularArray<T> {
    private T[] _array;
    private int _index;

    public CircularArray(int size) {
        _array = new T[size];
        for (int i = 0; i < size; i++) _array[i] = default(T);
        _index = size - 1;
    }

    public void Add(T item) {
        _index = (_index + 1) % _array.Length;
        _array[_index] = item;
    }

    public T[] GetItems() {
        T[] result = new T[_array.Length];
        for (int i = 0; i < _array.Length; i++) {
            result[i] = _array[(_index - i + _array.Length) % _array.Length];
        }

        return result;
    }
}