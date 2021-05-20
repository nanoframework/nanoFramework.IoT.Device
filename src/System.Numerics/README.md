# Using System.Numercs with the nanoFramework

When adding this shared project to your own project, you need to manually update your own **.nfproj** project file manually to include:

```xml
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```

ou will also need to reference the [nanoFramework version](https://docs.nanoframework.net/api/nanoFramework.System.Math.html) of

```
System.Math
```

in your own project for it to compile, as this is required by **System.Numerics**.
