/// <summary>
/// C# Unsafe code and Pointers will be ignored in scans.
/// This Requires -unsafe added to the msc.rsp file, Unity 2017.1 or newer doesn't require this.
/// Decent Explanation of steps for Unity.
/// https://stackoverflow.com/questions/39132079/how-to-use-unsafe-code-unity/39132664#39132664
/// </summary>
public class UnsafePointer
{

#if UNITY_2017_1_OR_NEWER // comment out define once added -unsafe to msc.rsp
    unsafe public static Unsafe* unsafePointer;
#endif
}

public struct Unsafe
{
    public int test;
}
