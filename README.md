# Mirror External Weaver

Mirror does not like running as a BepInEx plugin to say the least. When you normally write code using the Unity editor for Mirror, it normally postprocesses your code with
Mirror.Weaver. This is an external version of that with no Unity dependency. It contains prebuilt Mirror dlls for type reference, and references a stubbed Unity dll for more type references (as a NuGet package).
MirrorExternalWeaver needs a pdb of your project dll, along with all your references. The directory your assembly is in is scanned for references by default. 

The syntax is: `MirrorExternalWeaver.exe modDll.dll additionalReferenceFolder`.

For the additional reference folder, it may be preferable to use your game's Managed folder. 

Also, you must add this snippet of code to your mod's Awake function as Unity doesn't process the annotation correctly when loaded from a BepInEx plugin. 
```
Assembly.GetExecutingAssembly().GetType("Mirror.GeneratedNetworkCode").GetMethod("InitReadWriters").Invoke(null, null);
```

Weaver has been ported from Mirror 26.2.2. It shouldn't be too hard to use a newer Mirror version but it should work as-is with somewhat newer mirror versions. (untested)
