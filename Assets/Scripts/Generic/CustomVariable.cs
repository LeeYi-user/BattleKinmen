using System;

public class CustomVariable<T>
{
    private T _value;

    public CustomVariable(T value)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            return _value;
        }

        set
        {
            if (!_value.Equals(value))
            {
                _value = value;
                OnValueChanged?.Invoke();
            }
        }
    }

    public Action OnValueChanged;
}
