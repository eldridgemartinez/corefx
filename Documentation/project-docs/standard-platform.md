# .NET Platform Standard

## Disclaimer
We're sharing early plans for the future of building .NET class libraries. To track the progress of this work refer to https://github.com/dotnet/corefx/issues/4367

**NOTE: The netstandard TFM does not work in packages as yet. It's still implemented as dotnet. Please follow the issues for more details and progress updates.**

## Why?
To provide a more concrete guarantee of binary portability to future .NET-capable platforms with an easier-to-understand platform versioning plan.

Today, Portable Class Libraries (PCL) target an intersection of APIs depending on your platform selection when making the project. This gives you a specific surface area that guarantees you work on the chosen platforms. Those combinations are precomputed to give you the right set of surface area. When these portable libraries are packaged into NuGet, they are expressed with a static set of frameworks e.g. **portable-net45+win8**. While this describes the intent that you want to run on .NET Framework 4.5 and Windows 8.0, it is also restrictive, since new platforms can appear in the future that are perfectly capable of running those PCLs but are blocked due to the platforms that were selected when the project was created. In fact, putting the portable dll inside of a folder with a static list of profiles *essentially* makes it platform-specific. It's no different than doing:

```
MyLibrary/net45/MyLibrary.dll
MyLibrary/win8/MyLibrary.dll
```

The biggest difference is that you wouldn't be able to consume it in a PCL project type.

The .NET Platform Standard version represents binary portability across platforms using a **single** moniker. They are an evolution of the existing Portable Class Library system. They are "open-ended" in that they aren't tied down to a static list of monikers like **portable-a+b+c** is.

.NET Platform Standard versions are not too different to the PortableXXX profiles people use today which get represented in NuGet as **portable-a+b+c** (eg. Profile111). The key difference is that the **single** .NET Platform Standard moniker evolves and versions linearly, such that NuGet and other tools can infer compatibility, i.e. newer .NET Platform Standard versions are compatible with older ones.


## Terms
- **.NET Platform Standard** - versioned sets of the reference assemblies that all .NET Platforms must support.
- **PCL** - Portable Class Library
- **Platform** - e.g. .NET Framework 4.5, Windows Phone 8.1
- **Reference Assembly** - An assembly that contains API surface only. There is no IL in the method bodies. It is used for compilation only, and cannot be used to run. Also commonly referred to as "Contracts".
- **Implementation Assembly** - An assembly that contains an implementation of a reference assembly. These can be implemented as standalone assemblies but can sometimes be anchored by a platform and cannot be updated without updating the platform.
- **Anchored Assembly** - An implementation assembly where at least one platform provides the implementation as part of the platform rather than on top of it. Such an "anchored assembly" may only be updated on that platform by updating the platform itself.
- **Multi-targeting** - to compile the same source code files to different target platforms, i.e. against different API sets
- **Standard Library** - A blessed set of core .NET API's and versions that are prescribed to be used and supported together. This includes all of the API's in the Platform Standard plus additional libraries that are core to .NET but built on top of the Platform Standard. More than one Standard Library version can support the same Platform Standard.

## Principles
- Platform owners implement reference assemblies from a particular .NET Platform Standard version.
- Platform owners may implement a subset of reference assemblies from a particular .NET Platform Standard version.
- Any change in a reference assembly's API surface causes the .NET Platform Standard to version.
- Lower versions are always compatible with higher versions.

## Relationship to Platforms
The .NET Platform Standard is not a platform in and of itself. It is a standard that platforms are implemented against. The .NET Platform Standard defines reference assemblies (contracts) that platforms implement. [These contracts are defined by CoreFX](https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/standard-platform.md#list-of-net-corefx-apis-and-their-associated-net-standard-platform-version-tentative) and are either implemented in CoreFX as stand-alone managed code, or provided by a specific platform. For example, [System.Xml.XDocument](https://github.com/dotnet/corefx/tree/master/src/System.Xml.XDocument/src) is implemented as stand-alone managed code in CoreFx, but is an anchored assembly in .NET Framework.

![Platform Standard and Library](https://cloud.githubusercontent.com/assets/8228359/11072769/52bc69d6-879c-11e5-8640-d92f9ee1e2d9.png)

This picture is intended to show the relationship between the Platform Standard, the Standard Library, and the Anchor Platforms. The horizontal row titled "Platform Version" represents those things that, when impacted by a change, will cause support for one or more platforms to be dropped. The vertical column titled "Standard Library" represents the set of things platforms must support plus the set of libraries that are core to .NET but can update without dropping support for any platforms. More details on the Standard Library coming soon...

Each platform has it own rules for how it anchors certain assemblies. In .NET Framework, every API is anchored. On CoreCLR and Mono implementations of .NET Core, everything in mscorlib (the core assembly) is anchored. The challenge with anchored assemblies is that when they update, by definition they need to drop support for existing platforms where they are anchored because the platform copy will win. In order for a platform to declare that it supports a new Platform Standard, it must bring the API's in the Standard up to the most current version as well as the anchored API's that have been updated. Platforms that anchor and owners of the libraries that get anchored will need to coordinate how updates are made.

## Mapping the .NET Platform Standard to platforms

In general, class libraries which target a lower .NET Platform Standard version, like 1.0, can be loaded by the largest number of existing platforms, but will have access to a smaller set of APIs. On the other hand, class libraries which target a higher .NET Platform Standard version, like 1.3, can be loaded by a smaller number of newer platforms, but will have access to a larger, more recent set of APIs.

| Target Platform Name | Alias |  |  |  |  |  |
| :---------- | :--------- |:--------- |:--------- |:--------- |:--------- |:--------- |
|.NET Platform Standard | netstandard | 1.0 | 1.1 | 1.2 | 1.3 | 1.4 |
|.NET Framework|net|⇢|⇢|⇢|⇢|4.6.x|
|||⇢|⇢|⇢|4.6||
|||⇢|⇢|4.5.2||
|||⇢|⇢|4.5.1||
|||⇢|4.5|||
|Universal Windows Platform|uap|⇢|⇢|⇢|10.0|
|Windows|win|⇢|⇢|8.1||
|||⇢|8.0|||
|Windows Phone|wpa|⇢|⇢|8.1||
|||⇢|8.0|||
|Windows Phone Silverlight|wp|8.1||||
|||8.0||||
|DNX Core|dnxcore|⇢|⇢|⇢|⇢|5.0|
|Mono/Xamarin Platforms||⇢|⇢|⇢|⇢|*|
|Mono||⇢|⇢|*|||

### Observations

- If a library targets .NET Platform Standard version 1.3, it can *only* run on .NET Framework 4.6 or later, Universal Windows Platform 10 (UWP), DNX Core 5.0 and Mono/Xamarin platforms. 
- If a library targets .NET Platform Standard version 1.3, it can consume libraries from all previous .NET Platform Standard versions (1.2, 1.1, 1.0).
- The earliest .NET Framework to support a .NET Platform Standard version is .NET Framework 4.5. This is because the new portable API surface area (aka **System.Runtime** based surface area) that is used as the foundation for the .NET Platform Standard only became available in that version of .NET Framework. Targeting .NET Framework <= 4.0 requires multi-targeting.
- Each .NET Platform Standard version enables more API surface, which means it's available on fewer platforms. As the platforms update, their newer versions jump up into newer .NET Platform Standard versions.
- Platforms which have stopped updating -- like Silverlight on Windows Phone -- will only ever be available in the earliest .NET Platform Standard versions.

### Portable Profiles

PCL projects will be able to consume packages built for .NET Platform Standard (netstandard1.x).  The table below outlines the mapping of PCL portable profiles to the supported .NET Platform Standard version.  In order to ease transition to .NET Platform Standard, packages targeting it will be able to depend on PCL packages through the `imports` element (see [docs](http://docs.nuget.org/consume/projectjson-format)).  Since `portable-` packages don't include their dependencies explicitly, a .NET Platform Standard package with a dependency on a `portable-` package will need to include those dependencies itself.

| Profile | .NET Platform Standard version |
| ---------| --------------- |
| Profile7  .NET Portable Subset (.NET Framework 4.5, Windows 8) | 1.1 |
| Profile31 .NET Portable Subset (Windows 8.1, Windows Phone Silverlight 8.1)| 1.0 |
| Profile32 .NET Portable Subset (Windows 8.1, Windows Phone 8.1) | 1.2 |
| Profile44 .NET Portable Subset (.NET Framework 4.5.1, Windows 8.1) | 1.2 |
| Profile49 .NET Portable Subset (.NET Framework 4.5, Windows Phone Silverlight 8) | 1.0 |
| Profile78 .NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone Silverlight 8) | 1.0 |
| Profile84 .NET Portable Subset (Windows Phone 8.1, Windows Phone Silverlight 8.1) | 1.0 |
| Profile111 .NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone 8.1) | 1.1 |
| Profile151 .NET Portable Subset (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1) | 1.2 |
| Profile157 .NET Portable Subset (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1) | 1.0 |
| Profile259 .NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8) | 1.0 |

**NOTE: Xamarin Platforms augment the existing profile numbers above.**

Exising PCL projects in VS2013 and VS2015 (excluding UWP targets), can only target up to .NET Platform Standard version 1.2. To build libraries for .NET Platform Standard version >= 1.3 you have 2 options:

- Use project.json in csproj-based projects
- Use xproj-based projects, i.e. "Class Library (Package)" project template

## NuGet

### .NET Platform Standard version mapping

| .NET Platform Standard version | NuGet identifier |
| ---------| --------------- |
| 1.0 - 1.4 | netstandard1.0 - netstandard1.4 |

### Specific platform mapping

| Platform | NuGet identifier |
| ---------| --------------- |
| .NET Framework 2.0 - 4.6 | net20 - net46 |
|.NET Micro Framework | netmf |
| Windows 8 | win8, netcore45 |
| Windows 8.1 | win8, netcore451 |
| Windows Phone Silverlight (8, 8.1) | wp8, wp81 |
| Windows Phone 8.1 | wpa8.1 |
| Universal Windows Platform 10 | uap10, netcore50 |
| DNX on .NET Framework 4.5.1 - 4.6 | dnx451 - dnx46 |
| DNX on .NET Core 5.0 | dnxcore50  |
| Silverlight 4, 5 | sl4, sl5 |
| MonoAndroid | monoandroid |
| MonoTouch | monotouch |
| MonoMac | monomac |
| Xamarin iOS | xamarinios |
| Xamarin PlayStation 3 | xamarinpsthree | 
| Xamarin PlayStation 4 | xamarinpsfour |
| Xamarin PlayStation Vita | xamarinpsvita |
| Xamarin Watch OS | xamarinwatchos |
| Xamarin TV OS | xamarintvos |
| Xamarin Xbox 360 | xamarinxboxthreesixty |
| Xamarin Xbox One | xamarinxboxone |

### Deprecated monikers

| Platform | NuGet identifier |
| ---------| --------------- |
| ASP.NET 5.0 on .NET Framework | aspnet50 |
| ASP.NET 5.0 on .NET Core | aspnetcore50 |
| Windows 8 | winrt |

### Package authoring
When building a NuGet package, specifying folders named for platform monikers is enough to indicate what platforms your package targets.

MyPackage
```
MyPackage/lib/netstandard1.3/MyPackage.dll
```

The above package targets .NET Platform 1.3 (.NET Platform Standard 1.3)

#### Migrating existing PCLs in NuGet packages
Using the table outlined above, use the profile number of the csproj used to build the portable assembly to determine what NuGet folder it should go into. For example, **Newtonsoft.Json 7.0.1** has 2 portable folders:

```
Newtonsoft.Json/7.0.1/lib/portable-net40+sl5+wp80+win8+wpa81/Newtonsoft.Json.dll
Newtonsoft.Json/7.0.1/lib/portable-net45+wp80+win8+wpa81+dnxcore50/Newtonsoft.Json.dll
```

Only the second of these can be converted to a netstandard1.x based reference, because the first one target .NET 4.0, which is not supported in the .NET Platform Standard. Based on this csproj, we can see that the second PCL project is really profile 259.

https://github.com/JamesNK/Newtonsoft.Json/blob/d4916a76b5ed94342944cc665372dcc5dbd9e389/Src/Newtonsoft.Json/Newtonsoft.Json.Portable.csproj#L12

```
Newtonsoft.Json/7.0.1/lib/portable-net40+sl5+wp80+win8+wpa81/Newtonsoft.Json.dll
Newtonsoft.Json/7.0.1/lib/netstandard1.0/Newtonsoft.Json.dll
```

#### Generating dependency references

Unlike previous PCL packages, targeting the .NET Platform Standard requires the package dependencies to be fully specified. The specific version of the dependency doesn't matter but stating the dependency does. To aid in making this simple in the short term [Oren Novotny](https://github.com/onovotny) built a tool that can be used to generate the correct depenencies for nuspec metadata for your .NET Platform Standard based projects/assemblies:

https://github.com/onovotny/ReferenceGenerator

We expect to have something like this built into the Visual Studio project system as a first class experience in the future.

#### Bait and switch

**PCLCrypto** is a popular NuGet package that provides portable surface area via the [bait and switch technique](http://ericsink.com/entries/pcl_bait_and_switch.html). Usually, it's done with a number of platform-specific folders (the switch) and a portable folder (the bait) that is used for compilation (reference assembly):

```
PCLCrypto/1.0.80/lib/Xamarin.iOS/PCLCrypto.dll
PCLCrypto/1.0.80/lib/monoandroid/PCLCrypto.dll
PCLCrypto/1.0.80/lib/monotouch/PCLCrypto.dll
PCLCrypto/1.0.80/lib/win81/PCLCrypto.dll
PCLCrypto/1.0.80/lib/wp8/PCLCrypto.dll
PCLCrypto/1.0.80/lib/wpa81/PCLCrypto.dll
PCLCrypto/1.0.80/lib/portable-win8+wpa81+wp80+MonoAndroid10+xamarinios10+MonoTouch10/PCLCrypto.dll
```

When referencing this library from a PCL project, the portable-* dll is used for compilation. This is to allow other PCLs to be written against a consistent surface area across platforms. When referencing this package from a specific platform, the platform-specific implementation is chosen.

With .NET Platform Standard versions, and NuGet v3, we have introduced a more formal approach to making these kinds of packages. PCLCrypto would change to look like the following:

```
PCLCrypto/1.0.80/lib/Xamarin.iOS/PCLCrypto.dll
PCLCrypto/1.0.80/lib/monoandroid/PCLCrypto.dll
PCLCrypto/1.0.80/lib/monotouch/PCLCrypto.dll
PCLCrypto/1.0.80/lib/win81/PCLCrypto.dll
PCLCrypto/1.0.80/lib/wp8/PCLCrypto.dll
PCLCrypto/1.0.80/lib/wpa81/PCLCrypto.dll
PCLCrypto/1.0.80/ref/netstandard1.0/PCLCrypto.dll
```

The `ref` folder (`ref` being short for "reference assembly") is used to instruct the compiler what assembly should be used for compilation. The .NET Platform Standard version should be chosen such that it covers all of the specific platforms in the package (as indicated by the other sub-folders of "lib").

### Guard rails (supports)
In order to support platforms that implement a subset of the reference assemblies in a .NET Platform Standard version, **guard rails** were introduced to help class library authors predict where their libraries will run. As an example, let's introduce a new platform: **.NET Banana 1.0**. **.NET Banana 1.0** indicates it is based on .NET Platform Standard 1.3, but it did not implement the `System.AppContext` reference assembly. Class libraries authors targeting .NET Platform Standard version 1.3 need to know that their package may not work on **.NET Banana 1.0**.

```JSON
{
   "supports": [
      ".NET Banana 1.0"
   ],
   "dependencies": {
      "System.AppContext": "5.0.0"
   },
   "frameworks": {
      "netstandard1.3": { }
   } 
}
```

The above `project.json` will cause NuGet to do a compatibiltiy check, enforcing that an implementation assembly for `System.AppContext` can be found on **.NET Banana 1.0**. If this dependency check fails, you have 2 options:

1. Don't support **.NET Banana 1.0**
2. Multi-target for **.NET Banana 1.0** by adding that framework **explicitly** (this is only supported in xproj today) and use the platform-specific alternative to the `System.AppContext` API (if one exists).
```JSON
{
   "frameworks": {
      "netstandard1.3": { 
         "dependencies": {
            "System.AppContext": "5.0.0"
         }
      },
      "netbanana1.0": { }
   } 
}
```

## Required Tools
Tooling support for the `netstandard` TFM is as follows. This list will be updated regularly to reflect current status.

- **Visual Studio 2015**: With NuGet Extension `<TBD>`
- **Visual Studio 2013**: With NuGet Extension `<TBD>` 
- **Visual Studio 2012**: With NuGet Extension `<TBD>` 
- **NuGet CLI 3.x**: `<TBD>`
- **NuGet CLI 2.x**: `<TBD>`
- **DNU**: `<TBD>`
- **Xamarin Studio**: `<TBD>`

## List of .NET CoreFx APIs and their associated .NET Platform Standard version (tentative)

### Legend 
- `X` - API appeared in specific .NET Platform Standard version
- `⇠` - API version determined by nearest `X` e.g. In the table below, if you target .NET Platform Standard version 1.4 and reference Microsoft.CSharp, you'd get the 1.0 API version.

| Contract | 1.0 | 1.1 | 1.2 | 1.3 | 1.4 |
| -------- | --- | --- | --- | --- | --- |
| Microsoft.CSharp | X | ⇠ | ⇠ | ⇠ | ⇠ |
| Microsoft.VisualBasic |  | X | ⇠ | ⇠ | ⇠ |
| Microsoft.Win32.Primitives |  |  |  | X | ⇠ |
| Microsoft.Win32.Registry |  |  |  | X | ⇠ |
| Microsoft.Win32.Registry.AccessControl |  |  |  | X | ⇠ |
| System.AppContext |  |  |  | X | ⇠ |
| System.Collections | X | ⇠ | ⇠ | X | ⇠ |
| System.Collections.Concurrent |  | X | ⇠ | X | ⇠ |
| System.Collections.NonGeneric |  |  |  | X | ⇠ |
| System.Collections.Specialized |  |  |  | X | ⇠ |
| System.ComponentModel | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.ComponentModel.Annotations |  | X | ⇠ | X | ⇠ |
| System.ComponentModel.EventBasedAsync | X | ⇠ | ⇠ | X | ⇠ |
| System.ComponentModel.Primitives |  |  |  | X | ⇠ |
| System.ComponentModel.TypeConverter |  |  |  | X | ⇠ |
| System.Console |  |  |  | X | ⇠ |
| System.Data.Common |  |  |  | X | ⇠ |
| System.Data.SqlClient |  |  |  | X | ⇠ |
| System.Diagnostics.Contracts | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Diagnostics.Debug | X | ⇠ | ⇠ | X | ⇠ |
| System.Diagnostics.FileVersionInfo |  |  |  | X | ⇠ |
| System.Diagnostics.Process |  |  |  | X | X |
| System.Diagnostics.StackTrace |  |  |  | X | ⇠ |
| System.Diagnostics.TextWriterTraceListener |  |  |  | X | ⇠ |
| System.Diagnostics.Tools | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Diagnostics.TraceSource |  |  |  | X | ⇠ |
| System.Diagnostics.Tracing |  | X | X | X | ⇠ |
| System.Dynamic.Runtime | X | ⇠ | ⇠ | X | ⇠ |
| System.Globalization | X | ⇠ | ⇠ | X | ⇠ |
| System.Globalization.Calendars |  |  |  | X | ⇠ |
| System.Globalization.Extensions |  |  |  | X | ⇠ |
| System.IO | X | ⇠ | ⇠ | X | ⇠ |
| System.IO.Compression |  | X | ⇠ | X | ⇠ |
| System.IO.Compression.ZipFile |  |  |  | X | ⇠ |
| System.IO.FileSystem |  |  |  | X | ⇠ |
| System.IO.FileSystem.AccessControl |  |  |  | X | ⇠ |
| System.IO.FileSystem.DriveInfo |  |  |  | X | ⇠ |
| System.IO.FileSystem.Primitives |  |  |  | X | ⇠ |
| System.IO.FileSystem.Watcher |  |  |  | X | ⇠ |
| System.IO.IsolatedStorage | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.IO.MemoryMappedFiles |  |  |  | X | ⇠ |
| System.IO.Pipes |  |  |  | X | ⇠ |
| System.IO.UnmanagedMemoryStream |  |  |  | X | ⇠ |
| System.Linq | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Linq.Expressions | X | ⇠ | ⇠ | X | ⇠ |
| System.Linq.Parallel |  | X | ⇠ | ⇠ | ⇠ |
| System.Linq.Queryable | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Net.Http |  | X | ⇠ | ⇠ | ⇠ |
| System.Net.Http.Rtc |  | X | ⇠ | ⇠ | ⇠ |
| System.Net.Http.WinHttpHandler |  |  |  | X | ⇠ |
| System.Net.NameResolution |  |  |  | X | ⇠ |
| System.Net.NetworkInformation | X | ⇠ | ⇠ | X | ⇠ |
| System.Net.Primitives | X | X | ⇠ | X | ⇠ |
| System.Net.Requests | X | X | ⇠ | X | ⇠ |
| System.Net.Security |  |  |  | X | ⇠ |
| System.Net.Sockets |  |  |  | X | X |
| System.Net.Utilities |  |  |  | X | ⇠ |
| System.Net.WebHeaderCollection |  |  |  | X | ⇠ |
| System.Net.WebSockets |  |  |  | X | ⇠ |
| System.Net.WebSockets.Client |  |  |  | X | ⇠ |
| System.Numerics.Vectors |  |  |  | X | ⇠ |
| System.ObjectModel | X | ⇠ | ⇠ | X | ⇠ |
| System.Reflection | X | ⇠ | ⇠ | X | ⇠ |
| System.Reflection.Context |  | X | ⇠ | ⇠ | ⇠ |
| System.Reflection.DispatchProxy |  |  |  | X | ⇠ |
| System.Reflection.Emit |  | X | ⇠ | ⇠ | ⇠ |
| System.Reflection.Emit.ILGeneration | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Reflection.Emit.Lightweight | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Reflection.Extensions | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Reflection.Primitives | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Reflection.TypeExtensions |  |  |  | X | ⇠ |
| System.Resources.ReaderWriter |  |  |  | X | ⇠ |
| System.Resources.ResourceManager | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Runtime | X | ⇠ | X | X | ⇠ |
| System.Runtime.CompilerServices.VisualC |  |  |  | X | ⇠ |
| System.Runtime.Extensions | X | ⇠ | ⇠ | X | ⇠ |
| System.Runtime.Handles |  |  |  | X | ⇠ |
| System.Runtime.InteropServices |  | X | X | X | ⇠ |
| System.Runtime.InteropServices.RuntimeInformation |  | X | ⇠ | ⇠ | ⇠ |
| System.Runtime.InteropServices.WindowsRuntime | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Runtime.Loader | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Runtime.Numerics |  | X | ⇠ | ⇠ | ⇠ |
| System.Runtime.Serialization.Json | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Runtime.Serialization.Primitives | X | ⇠ | ⇠ | X | ⇠ |
| System.Runtime.Serialization.Xml | X | ⇠ | ⇠ | X | ⇠ |
| System.Runtime.WindowsRuntime | X | ⇠ | X | ⇠ | ⇠ |
| System.Runtime.WindowsRuntime.UI.Xaml |  | X | ⇠ | ⇠ | ⇠ |
| System.Security.AccessControl |  |  |  | X | ⇠ |
| System.Security.Claims |  |  |  | X | ⇠ |
| System.Security.Cryptography.Algorithms |  |  |  | X | ⇠ |
| System.Security.Cryptography.Cng |  |  |  | X | ⇠ |
| System.Security.Cryptography.Csp |  |  |  | X | ⇠ |
| System.Security.Cryptography.Encoding |  |  |  | X | ⇠ |
| System.Security.Cryptography.OpenSsl |  |  |  | X | ⇠ |
| System.Security.Cryptography.Primitives |  |  |  | X | ⇠ |
| System.Security.Cryptography.X509Certificates |  |  |  | X | ⇠ |
| System.Security.Principal | X | ⇠ | ⇠ | ⇠ | ⇠ |
| System.Security.Principal.Windows |  |  |  | X | ⇠ |
| System.ServiceModel.Duplex |  | X | ⇠ | ⇠ | ⇠ |
| System.ServiceModel.Http | X | X | ⇠ | X | ⇠ |
| System.ServiceModel.NetTcp |  | X | ⇠ | X | ⇠ |
| System.ServiceModel.Primitives | X | X | ⇠ | X | ⇠ |
| System.ServiceModel.Security | X | X | ⇠ | ⇠ | ⇠ |
| System.ServiceProcess.ServiceController |  |  |  | X | ⇠ |
| System.Text.Encoding | X | ⇠ | ⇠ | X | ⇠ |
| System.Text.Encoding.CodePages |  |  |  | X | ⇠ |
| System.Text.Encoding.Extensions | X | ⇠ | ⇠ | X | ⇠ |
| System.Text.RegularExpressions | X | ⇠ | ⇠ | X | ⇠ |
| System.Threading | X | ⇠ | ⇠ | X | ⇠ |
| System.Threading.AccessControl |  |  |  | X | ⇠ |
| System.Threading.Overlapped |  |  |  | X | ⇠ |
| System.Threading.Tasks | X | ⇠ | ⇠ | X | ⇠ |
| System.Threading.Tasks.Parallel |  | X | ⇠ | ⇠ | ⇠ |
| System.Threading.Thread |  |  |  | X | ⇠ |
| System.Threading.ThreadPool |  |  |  | X | ⇠ |
| System.Threading.Timer |  |  | X | ⇠ | ⇠ |
| System.Xml.ReaderWriter | X | ⇠ | ⇠ | X | ⇠ |
| System.Xml.XDocument | X | ⇠ | ⇠ | X | ⇠ |
| System.Xml.XmlDocument |  |  |  | X | ⇠ |
| System.Xml.XmlSerializer | X | ⇠ | ⇠ | X | ⇠ |
| System.Xml.XPath |  |  |  | X | ⇠ |
| System.Xml.XPath.XDocument |  |  |  | X | ⇠ |
| System.Xml.XPath.XmlDocument |  |  |  | X | ⇠ |
