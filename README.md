# Zombie Object Detector

Scripts to detect and diagnose zombie objects.

## Description of Zombie Objects

The best description of what we mean by a "zombie" object is in a 2014 blog post here https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/.  Specifically the paragraph labelled "purpose two".

To recap:  Many C# classes in the `UnityEngine` namespace act as wrappers to instances of corresponding C++ classes, which hold the actual data.  The class `UnityEngine.GameObject` is an example.  Zombie objects are instances of these C# classes that have outlived their C++ counterpart.  The C++ instances are destroyed in an entirely deterministic fashion, for example on scene transition.  On the other hand the C# instances are garbage collected just like any other C# object.  This means that there is an opportunity for the C# object to remain in existence by being referenced by other objects, static variables, etc.


## How this tool determines Zombie Objects

The tool attempts to traverse every instance of every class.  From any one object, it uses reflection to obtain every field of that object, and simply recurses into each one.  It does this until it has examined every object in memory.

An object is considered a zombie if it satisfies all of the following criteria:
* It is not *actually* null.
* It is a subclass of `UnityEngine.Object`.
* When you compare it to null using `(obj as UnityEngine.Object) == null`, it returns `true`.

# Installation

* Install the package using the Unity Package Manager window, using `Add Package from Git URL`.

This repository contains an example project that contains various examples of Zombie objects.  These examples are simple and easy to fix.  In larger projects this is not always the case. This tool will help you find them, and give you a trace of where and what is keeping them alive. 

# Included Examples:

* Unsafe pointer: requires Unity 2017.1 or for the msc.rsp file to be declared with -unsafe. [https://stackoverflow.com/questions/39132079/how-to-use-unsafe-code-unity/39132664#39132664](https://stackoverflow.com/questions/39132079/how-to-use-unsafe-code-unity/39132664#39132664) 

* BadEqualImplementation: Equals is overridden with a bad example that causes it to get ignored.

* FalsePositive: Override of ToString() causing zombie check to fail.

* IgnoredTypeParam: IgnoredTypeParam<T> : MonoBehaviour where T : MonoBehaviour, the derived classes are still scanned, and all member variables still getting scanned.

* Arrays, Lists, Stacks and Dictionaries:

* Events: Lambda and normal function attachment.

* References: static and sub references


# Credits

The project was started by Stewart McCready ([@StewMcc](https://github.com/StewMcc)) during his internship at Unity, with subsequent work by Peter Pimley ([@peter-pimley-unity](https://github.com/peter-pimley-unity)).
