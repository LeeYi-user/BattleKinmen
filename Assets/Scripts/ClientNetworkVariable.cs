using System;

public class ClientNetworkVariable<T>
{
    private T _value;

    public ClientNetworkVariable(T value)
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
