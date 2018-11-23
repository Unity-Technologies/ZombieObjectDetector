
public class BadEqualsImplementation
{
    protected static BadEquals m_BadEquals = new BadEquals();
}

/// <summary>
/// Bad implementation of .Equals.
/// <para>
/// Means it will fail when .Equals is invoked against something other than "BadEquals".
/// This means we can't check if we have already scaned the object, so we just ignore it.
/// </para>
/// Logged with <seealso cref="CSharpZombieDetector.ZombieObjectDetector.LoggingOptions.kListBadEqualsFunctionImplementation"/>
/// </summary>
public struct BadEquals
{
    public int x;
    public int y;
    public int z;

    // Example Taken From bug found in 3rdparty tool.
    public override bool Equals(System.Object o)
    {
        if (o == null)
            return false;
        BadEquals rhs = (BadEquals)o; // throws an exception when not BadEquals.

        return x == rhs.x &&
               y == rhs.y &&
               z == rhs.z;
    }

    public override int GetHashCode()
    {
        return x * 73856093 ^ y * 19349663 ^ z * 83492791;
    }
}
