# Zombie Object Detector

Scripts to detect and diagnose zombie objects.

## Description of Zombie Objects

The best description of what we mean by a "zombie" object is in a 2014 blog post here https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/.  Specifically the paragraph labelled "purpose two".

To recap:  Many C# classes in the `UnityEngine` namespace act as wrappers to instances of corresponding C++ classes, which hold the actual data.  The class `UnityEngine.GameObject` is an example.  Zombie objects are instances of these C# classes that have outlived their C++ counterpart.  The C++ instances are destroyed in an entirely deterministic fashion, for example on scene transition.  On the other hand the C# instances are garbage collected just like any other C# object.  This means that there is an opportunity for the C# object to remain in existence by being referenced by other objects, static variables, etc.


## How this tool determines Zombie Objects

The tool attempts to traverse every instance of every class.  From any one object, it uses reflection to obtain every field of that object, and simply recurses into each one.  It does this until it has examined every object in memory.

An object is considered a zombie if its `ToString` function returns the string `"null"`. (i.e. the string containing the four characters `n`, `u`, `l`, `l`, rather than a null string).

**TODO** There is a better way that does not generate a temporary string:

```
GameObject go = ... ;
object o = go;
bool zombie = (go == null) && (o != null);
```


# Installation

* Copy the `.cs` files from this repository into your own project.

This repository contains an example project that contains various examples of Zombie objects.  These examples are simple and easy to fix.  In larger projects this is not always the case. This tool will help you find them, and give you a trace of where and what is keeping them alive. 

# Usage

## Basic In-Editor Tests

* Run the game in editor, and load and unload scenes.

* Right click in the scene hierarchy and add a zombie detector.

* Select the Zombie Detector, and in the inspector click Log Zombies.

* Wait for it to complete, console will show all zombies found, it will also be saved to a log file.

## In-Game Tests

* Right click in the scene hierarchy of a scene that will persist between loads and add a zombie detector.

* Set the desired log key in the zombie detector's inspector.

* Build and launch the game.

* Load scenes as normal

* Press Log Zombie Key

* This will be saved to the .ZombieLogs folder (dependent on platform)
    * Note: It will fail to write if it doesn't have permissions to dataPath(i.e. Packaged Console Builds) It will still output to the Debug Console however.

## Log Save Location

Logs are written to `Application.dataPath`, in a subdirectory called `.ZombieLogs`.


# Settings

## Logging Options

| Option| Info |
|:---|:---| 
| IgnoredMembers| Shows member variables that have been ignored. |
| FieldInfoNotFound| Shows reflection fieldinfo on a member that hasn't been found, this usually means reflection is confused about what type of member it is. |
| MemberType| Shows member variables type: Field, Method, etc. |
| FieldType| Shows what type of field it is; Array or default Other. |
| AlreadySearched| Shows objects that have already been searched, so wont be searched again. |
| InvalidType| Shows types that are considered invalid, NumericTypes, Pointers, GenericParameters, and anything in the Ignored types list. |
| Exceptions| Shows exceptions that can generally be ignored, for example the exception caused by a bad Equals implementation. |
| ListBadEquals| Shows a list of all classes that have bad implementations of the Equals function. |
| ListScannedObjects| Shows a list of all objects scanned (Not All types*) |
| StaticFieldZombieCount| Shows how many objects each static field has connected to it. |
| FullZombieStackTrace| Outputs the full stack trace from zombie object all the way back to the original static field. |



## Log Tag

Inserts the tag into the filename.

## Types to Scan

The whitelist of types that will have their initial statics scanned, it doesn’t reduce subsequent scans. Useful if wishing to only check a specific static field’s zombies. Leave empty to scan all objects.

## Types to ignore

The blacklist of types that will be ignored at every stage, initial statics and all subsequent object scans.

## Zombie Logging Key Code

A simple way of hooking up a keycode for starting zombie logging. 

# Included Examples:

* Unsafe pointer: requires Unity 2017.1 or for the msc.rsp file to be declared with -unsafe. [https://stackoverflow.com/questions/39132079/how-to-use-unsafe-code-unity/39132664#39132664](https://stackoverflow.com/questions/39132079/how-to-use-unsafe-code-unity/39132664#39132664) 

* BadEqualImplementation: Equals is overridden with a bad example that causes it to get ignored.

* FalsePositive: Override of ToString() causing zombie check to fail.

* IgnoredTypeParam: IgnoredTypeParam<T> : MonoBehaviour where T : MonoBehaviour, the derived classes are still scanned, and all member variables still getting scanned.

* Arrays, Lists, Stacks and Dictionaries:

* Events: Lambda and normal function attachment.

* References: static and sub references


# Credits

Most of the work was done by Stewart McCready (@StewMcc), with some help by Peter Pimley (@peter-pimley-unity).
