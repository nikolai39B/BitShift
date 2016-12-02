using System;
using System.Collections;

public class Tuple<T, U> 
    where T : IEquatable<T>
    where U : IEquatable<U>
{
    /// <summary>
    /// Create a new tuple with two values.
    /// </summary>
    /// <param name="first">The first value, of the first type.</param>
    /// <param name="second">The second value, of the second type.</param>
    public Tuple(T first, U second)
    {
        First = first;
        Second = second;
    }

    //-----------//
    // Overrides //
    //-----------//
    public override bool Equals(object obj)
    {
        // Check null
        if (obj == null)
        {
            return false;
        }

        // Check type
        Tuple<T, U> objAsTuple = obj as Tuple<T, U>;
        if (objAsTuple == null)
        {
            return false;
        }

        return First.Equals(objAsTuple.First) && Second.Equals(objAsTuple.Second);
    }

    public static bool operator ==(Tuple<T, U> first, Tuple<T, U> second)
    {
        if (object.ReferenceEquals(first, null))
        {
            return object.ReferenceEquals(second, null);
        }

        return first.Equals(second);
    }

    public static bool operator !=(Tuple<T, U> first, Tuple<T, U> second)
    {
        return !(first == second);
    }

    public override int GetHashCode()
    {
        return First.GetHashCode() ^ Second.GetHashCode();
    }

    //------//
    // Data //
    //------//
    public T First { get; private set; }
    public U Second { get; private set; }
}
