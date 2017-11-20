[![Version](https://img.shields.io/nuget/v/OhadSoft.ET4W.svg)](https://www.nuget.org/packages/OhadSoft.ET4W/)

# ET4W
ET4W is a [T4 Text Template](https://msdn.microsoft.com/en-us/library/bb126445.aspx) code generator for C# [ETW (Event Tracing for Windows)](https://msdn.microsoft.com/en-us/library/dn774985(v=pandp.20).aspx#_Overview_of_custom) classes.

By authoring a simple JSON file, you hit the ground running with full-fledged [`EventSource`](https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventsource(v=vs.110).aspx) classes, generated using [ETW best practices](http://blogs.msmvps.com/kathleen/2014/01/24/how-are-event-parameters-best-used-to-create-an-intuitive-custom-evnetsourcetrace/). These classes are further wrapped in helper classes that support custom types and common parameters, ready for immediate logging.

A full [JSON schema](https://github.com/ohadschn/ET4W/blob/master/src/events-schema.json) is included for in-editor validation and auto-completion (supported in Visual Studio and other editors). Further validation takes place at generation time.

![JSON editing experience](https://raw.githubusercontent.com/ohadschn/ET4W/master/docs/Transformation.png)

## Why ET4W
ETW is the best tracing solution for the Windows platform, period. It is unmatched in performance and reliability. More and more tools are built to analyze its events to astounding depth. Microsoft explicitly recommends it for almost any logging purpose.

However, writing proper ETW classes is [tricky](https://msdn.microsoft.com/en-us/library/dn774985(v=pandp.20).aspx). It used to be [much worse](https://blogs.msdn.microsoft.com/seealso/2011/06/08/use-this-not-this-logging-event-tracing/) (before the advent of automatic manifest generation), but there's still a lot of code that has to be written manually in a very specific way. Not only is it tedious, it also leaves plenty of room for user error.
* You have to decorate each event method with [`EventAttribute`](https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventattribute(v=vs.110).aspx) (theoretically optional, but in practice you'll do this for every single method).
* You then have to call [WriteEvent](https://msdn.microsoft.com/en-us/library/hh393412(v=vs.110).aspx) in a very specific manner - the event ID and parameters must match exactly and in order.
* [Tasks](https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventtask(v=vs.110).aspx) and [Keywords](https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventkeywords(v=vs.110).aspx) should be specified using nested classes with a very specific structure.
* You are encouraged to expose the class in a very specific singleton pattern.
* You must be aware of the exact types supported by ETW (not documented anywhere, so you need to reflect it off `ManifestBuilder.GetTypeName`).
* If you want to log types that aren't supported, it's your responsibility to invoke the appropriate conversions (typically via a manually created wrapper class).
* If you want some common parameters to be present in every event, you'll have to add them manually to each method, and provide them manually in each call.

ET4W solves these issues for you. All you have to do is specify in the JSON the events you want (along with their tasks, keywords, parameters, etc). You can also define common parameters and custom type converters, and ET4W will take care of the rest.

As an added bonus, this approach allows you to leverage the same event JSON for cross-platform event consistency. By building similar generation scripts for other languages (VB, C++, etc), you could maintain a "single source of truth" for your events across various platforms (critical for telemetry measurements, for example).

## Usage
1. Install the *OhadSoft.ET4W* NuGet package: [https://www.nuget.org/packages/OhadSoft.ET4W](https://www.nuget.org/packages/OhadSoft.ET4W).
2. An *ET4W* folder will be added to your project, containing two files: 
   * *ET4W.ttinclude*
   * *events-schema.json*
3. Create a new JSON file and specify your events inside it according to the [schema](https://github.com/ohadschn/ET4W/blob/master/src/events-schema.json) (see the next section for a detailed walkthrough).
4. Create a new blank T4 Template (*.tt*) file, which will be used to generate your event classes.
5. Replace the contents of the *.tt* file you just created with the following generation code: 

   ```csharp
   <#@ template debug="false" hostspecific="true" language="C#" #>
   <#@ output extension=".cs" #>
   <#@ include file="ET4W\ET4W.ttinclude" #>
   <# WriteEventSource(Host.ResolvePath("events.json"), "MyNamespace"); #>
   ```
6. Customize the generation code (that you just pasted above):
   * If your *.tt* file is not located at the root of the project, you'll need to change the (relative) location of `ET4W.ttinclude` in the `<#@include#>` directive. For example, if your *.tt* file is located inside an `Events` subfolder, your directive should look like this: `<#@ include file="..\ET4W\ET4W.ttinclude" #>`.
   * Replace `events.json` in the `<# WriteEventSource #>` control block with your actual event JSON file name (created in step 3 above).
   * Replace `MyNamespace` with the namespace in which you'd like the generated event classes to reside.
7. [Transform](https://msdn.microsoft.com/en-us/library/dd820620.aspx) the *.tt* file.
   * Instead of transforming the file manually every time, consider using the [Clarius.TransformOnBuild](https://www.nuget.org/packages/Clarius.TransformOnBuild/) NuGet package, which will automatically transform your *.tt* template files upon each build. It will even work with automated (continuous integration) MSBuild builds, as it makes use of `TextTransform.exe` which should be included in every build machine. **Important**: At the time of writing, you must set each *.tt* file's *Build Action* to *None* in the file's properties for the automatic transformation to work (the default is *Content*).
8. The event classes should now be generated and ready for use. For example, if you specified `Foo` as the class name in the JSON, two classes will be generated:
    * `FooEventSource` - this is the raw event source class. Typically, you won't use it directly.
    * `FooEvents` - this is the wrapper event class, recommended for client use. It adds common parameter and custom type support.
9. (optional) Install recommended validation NuGet packages. While ET4W is [reasonably tested](https://github.com/ohadschn/ET4W/tree/master/src/Tests), additional validation can't hurt. Especially considering how ETW silently ignores errors and simply *discards* erroneous events - something you definitely don't want to find out a few months into production...
   * [Microsoft.Diagnostics.Tracing.EventRegister](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.EventRegister/) - this package will automatically validate all event source classes in the assembly as a post-build step.
   * [EnterpriseLibrary.SemanticLogging.EventSourceAnalyzer](https://www.nuget.org/packages/EnterpriseLibrary.SemanticLogging.EventSourceAnalyzer/) - the [SLAB](https://github.com/mspnp/semantic-logging) event source analyzer will allow you to write tests that validate event sources at runtime (in addition to the build-time validation performed by *Microsoft.Diagnostics.Tracing.EventRegister* mentioned above). Simply add the following line of code to your test: `EventSourceAnalyzer.InspectAll(MyGeneratedEventSource.Log)`. **Important**: At the time of writing, `DateTime` and `byte[]` parameters are not supported by the analyzer, and invoking it on event sources containing these types will throw. This issue should be resolved in upcoming versions of the package.

## Creating the event JSON and using the generated classes
In its most basic form, the event JSON is very simple. All you have to do is define the name prefix of the classes you want generated, the [ETW event source name](https://msdn.microsoft.com/en-us/library/dn774985(v=pandp.20).aspx#_Overview_of_custom), and some event you want to fire. It's also highly recommended to reference the provided schema for in-editor validation and auto-completion (supported in VS and other editors). Here's how it looks like:
```json
{
  "$schema": "ET4W/events-schema.json",
  "class": "Minimal",
  "sourceName": "OS-Test-Minimal",
  "events": [
    {
      "id": 1,
      "name": "Foo"
    }
  ]
}
```
Note that you'll have to adjust the schema's relative path if your JSON file is not located at the root of the project (similar to the adjustment you had to make in the *.tt* file's `<#@include#>` directive). Also note that it may be required to close and re-open the file in Visual Studio in order for schema validation and auto-completion to kick in. If your editor complains about a schema mismatch and you can't figure out why, try [this nifty little site](http://www.jsonschemavalidator.net/) which will provide you with the exact reason.

Once you transform the *.tt* template, two classes will be generated: the `MinimalEvents` wrapper class and the raw `MinimalEventSource`. As we mentioned above, it is generally recommended to use the wrapper class:
```csharp
var events = new MinimalEvents();
events.Foo();
```

### Event metadata
You might want to enrich your events with some more metadata:

* [Channels](https://msdn.microsoft.com/en-us/library/windows/desktop/dd996911(v=vs.85).aspx) can be used to target the audience of your events.
* [Severity Levels](https://msdn.microsoft.com/en-us/library/windows/desktop/dd996917(v=vs.85).aspx) can be used for severity or verbosity indication.
* [Keywords](https://msdn.microsoft.com/en-us/library/windows/desktop/dd996915(v=vs.85).aspx) can be used for event classification.
* [Tasks and Opcodes](https://msdn.microsoft.com/en-us/library/windows/desktop/dd996918(v=vs.85).aspx) can be used for logical event grouping.

You can also set event versions for manifest compatibility checks (more details [here](http://blogs.msmvps.com/kathleen/2014/01/24/how-are-event-parameters-best-used-to-create-an-intuitive-custom-evnetsourcetrace/)). Here's how an event with all possible metadata specified would look like:

```json
{
  "$schema": "ET4W/events-schema.json",
  "class": "Metadata",
  "sourceName": "OS-Test-Metadata",
  "keywords": [ "Key", "Word" ],
  "tasks": [ "Eat" ],
  "events": [
   {
    "id": 1,
    "name": "Foo",
    "version": 2,
    "channel": "Admin",
    "level": "Warning",
    "keywords": [ "Key", "Word" ],
    "task": "Eat",
    "opcode": "Info"
   }
  ]
}
```
Note the top-level `keywords` and `tasks` properties. They are required in order to allow event verification (inferring them implicitly from event definitions would mean that a typo could end up creating an unintended task or keyword).

Usage of the generated event class remains the same:
```csharp
var events = new MetadataEvents();
events.Foo();
```

### Parameters and messages
Parameters are a crucial part of most events. And when parameters are in play, one usually expects to find a formatted message, containing the values of said parameters for easier human consumption. Let's see how that might look like in the event JSON:
```json
{
  "$schema": "ET4W/events-schema.json",
  "class": "Params",
  "sourceName": "OS-Test-Params",
  "events": [
    {
      "id": 1,
      "name": "Foo",
      "message": "b: {0}, c: {1}, i: {2}",
      "parameters": [
        {
          "name": "b",
          "type": "Boolean"
        },
        {
          "name": "c",
          "type": "Char"
        },
        {
          "name": "i",
          "type": "Int32"
        }
      ]
    }
  ]
}
```
Note how the numbers in curly brackets (`{0}`, `{1}`, `{2}`) correspond to the parameters at the matching positions (just like `String.Format`). For the full list of possible types consult `etwNativeType` in the [schema](https://github.com/ohadschn/ET4W/blob/master/src/events-schema.json) (or invoke your editor's auto completion for any parameter's type).

Using the generated event class is straightforward:
```csharp
var events = new ParamsEvents();
events.Foo(true, 'a', 42);
```

### Custom types
Suppose that you'd like to log a parameter whose type isn't natively supported by ETW. All you have to do is declare it as a custom type, specify the target native ETW type (as it must ultimately be converted into an ETW-supported type in order to be logged), and configure it as the parameter's type:
```json
{
  "$schema": "ET4W/events-schema.json",
  "class": "CustomTypes",
  "sourceName": "OS-Test-CustomTypes",
  "customTypes": [
   {
       "fullyQualifiedName": "Tests.CustomType",
       "targetType": "String"
   }
  ],
  "events": [
    {
      "id": 1,
      "name": "Foo",
      "parameters": [
        {
          "name": "bar",
          "customType": "Tests.CustomType"
        }
      ]
    }
  ]
}
```
Note that much like tasks and keywords, custom types need to be defined in the top-most `customTypes` property in order to allow event parameter type validation (so that typos don't result in unintended parameter types).  Also note how the `customType` property is used to define the parameter's type (as opposed to the `type` property which is used for native ETW types).

As for the conversion to the target ETW-supported type, it is done using a *converter* function you must provide to the generated event class:
```csharp
class CustomType
{
    public string Str {get; set;}
}

var events = new CustomTypesEvents(customType => customType.Str);
events.Foo(new CustomType {Str = "Hello World"});
```

### Common parameters
In many cases, you might have some contextual information you would like attached to every event in a particular class (for example, a request or session ID). Adding such parameters to each and every event manually would be tedious, which is why ET4W supports *common parameters*. These parameters are automatically added to each event (preceding its explicitly defined parameters), and their value is either provided at the call site or calculated using a *generator* method. Here's a sample JSON with a pair of common parameters, one of which will be generated automatically in each call using a generator method:
```json
{
  "$schema": "ET4W/events-schema.json",
  "class": "CommonParams",
  "sourceName": "OS-Test-CommonParams",
  "customTypes": [
    {
      "fullyQualifiedName": "Tests.Session",
      "targetType": "GUID"
    }
  ],
  "commonParameters": [
    {
      "name": "requestId",
      "type": "GUID"
    },
    {
      "name": "sessionId",
      "customType": "Tests.Session",
      "generated": true
    }
  ],
  "events": [
    {
      "id": 1,
      "name": "Foo",
      "message": "requestId: {0}, sessionId: {1}, bar: {2}",
      "parameters": [
        {
          "name": "bar",
          "type": "String"
        }
      ]
    }
  ]
}
```
Note how common parameters can be custom types as well. Also note how common parameters ultimately end up as ETW parameters, just as if they were explicitly defined in each and every event (as its first parameters). This is why `{0}` represents `requestId` and `{1}` represents `sessionId` in the event's message.

As for the generator method, you must provide it to the generated event class (after the custom type converters):
```csharp
class Session
{
    public Guid Id {get; set;}
}

var events = new CommonParamsEvents(session => session.Id, () => GetSessionFromSomewhere());
events.Foo(m_requestId, "hello universe");
```
Note how the `requestId` and `bar` parameters are provided at the call site, whereas the `sessionId` parameter is generated automatically using the provided generator method (and in turn converted to a `GUID` by the provided converter method).

### Additional JSON samples
For more event JSON samples see: [https://github.com/ohadschn/ET4W/tree/master/src/Tests/Events](https://github.com/ohadschn/ET4W/tree/master/src/Tests/Events).

## Microsoft.Diagnostics.Tracing
The [Microsoft EventSource Library](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.EventSource) NuGet package contains an `EventSource` implementation that provides more features than its `System.Diagnostics.Tracing` parallel. For example, it can be used to bring channel support to projects targeting .NET 4.5.1 or earlier (since this feature was only introduced in .NET 4.6).

ET4W supports `Microsoft.Diagnostics.Tracing` by accepting an additional (optional) `useMicrosoftDiagnosticsTracing` parmeter in *ET4W.ttinclude*'s `WriteEventSource` method (used in the last control block of the event generation *.tt* file):

```csharp
<#@ template debug="false" hostspecific="true" language="C#" #>
<#@ output extension=".cs" #>
<#@ include file="ET4W\ET4W.ttinclude" #>
<# WriteEventSource(Host.ResolvePath("events.json"), "MyNamespace", useMicrosoftDiagnosticsTracing: true); #>
```

## Generating event sources that inherit from utility event source classes
Some advanced scenarios require the generated raw event source class to inherit from a custom class that in turn inherits from `EventSource` (rather than inherit from `EventSource` directly, as is done by default). ET4W supports these scenarios by accepting an additional (optional) `baseTypeFullyQualifiedName` parameter in *ET4W.ttinclude*'s `WriteEventSource` method (used in the last control block of the event generation *.tt* file):

```csharp
<#@ template debug="false" hostspecific="true" language="C#" #>
<#@ output extension=".cs" #>
<#@ include file="ET4W\ET4W.ttinclude" #>
<# WriteEventSource(Host.ResolvePath("events.json"), "MyNamespace", baseTypeFullyQualifiedName:"UtilEventSrc"); #>
```

For more information about utility event source classes and some of the scenarios in which they can come in handy, consult the *_EventSourceUsersGuide.docx* document in the [Microsoft.Diagnostics.Tracing.EventSource](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.EventSource/) package (the document is added to your project automatically when the package is installed). Example usage can be found in [baseClassEvents.tt](https://github.com/ohadschn/ET4W/blob/master/src/Tests/Events/baseClassEvents.tt) and [BaseClassEventsTests.cs](https://github.com/ohadschn/ET4W/blob/master/src/Tests/Suites/BaseClassEventsTests.cs). 

## ETW Resources
.NET's `EventSource` and ET4W take away most of the pain of using ETW, but a good understanding of its fundamentals is still required in order to use it effectively. The following blog post should help you hit the ground running, and includes links to various additional resources: [https://www.ohadsoft.com/2014/10/getting-started-with-etw-using-nets-eventsource/](https://www.ohadsoft.com/2014/10/getting-started-with-etw-using-nets-eventsource/).

## Powered by Resharper 
[![Resharper](https://raw.githubusercontent.com/ohadschn/ET4W/master/docs/icon_ReSharper.png)](https://www.jetbrains.com/resharper/)
