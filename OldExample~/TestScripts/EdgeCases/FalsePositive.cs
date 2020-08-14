/// <summary>
/// Overriding ToString and returning "null" specifically causes a false positive.
/// <para>
/// This is due to the way Zombie Objects are detected.
/// </para>
/// See <seealso cref="CSharpZombieDetector.ZombieObjectDetector.LogIsZombieObject(object)"/>
/// </summary>
public class FalsePositiveOverrideString
{

    public static FalsePositiveOverrideString falsePositiveExample = new FalsePositiveOverrideString();

    public override string ToString()
    {
        return "null";
    }
}
