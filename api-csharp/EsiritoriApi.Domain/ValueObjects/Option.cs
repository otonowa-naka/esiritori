namespace EsiritoriApi.Domain.ValueObjects;

public readonly struct Option<T>
{
    public bool HasValue { get; }
    public T Value { get; }

    private Option(T value, bool hasValue)
    {
        Value = value;
        HasValue = hasValue;
    }

    public Option(T value)
    {
        if (value == null)
        {
            Value = default(T)!;
            HasValue = false;
        }
        else
        {
            Value = value;
            HasValue = true;
        }
    }

    public static Option<T> None() => new(default(T)!, false);

    public static Option<T> Some(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "値がnullの場合はSomeを使用できません");
        return new(value, true);
    }

    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
    {
        return HasValue ? onSome(Value) : onNone();
    }

    public void Match(Action<T> onSome, Action onNone)
    {
        if (HasValue)
            onSome(Value);
        else
            onNone();
    }

    public Option<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return HasValue ? Option<TResult>.Some(mapper(Value)) : Option<TResult>.None();
    }

    public T GetValueOrDefault(T defaultValue = default(T)!)
    {
        return HasValue ? Value : defaultValue;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Option<T> other)
        {
            if (!HasValue && !other.HasValue)
                return true;
            if (HasValue && other.HasValue)
                return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HasValue ? EqualityComparer<T>.Default.GetHashCode(Value!) : 0;
    }

    public static bool operator ==(Option<T> left, Option<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Option<T> left, Option<T> right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return HasValue ? $"Some({Value})" : "None";
    }
}
