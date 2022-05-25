using System;

public class MetaData
{
    public string Value;
    public Type type = typeof(string);

}

public class MetaData<T> : MetaData
{
    public new T Value;

    public new Type type = typeof(T);
}