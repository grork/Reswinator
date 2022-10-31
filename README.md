Reswinator
==========

When using `resw` files for resources in applications targetting the Windows App
SDK, there is no simple way to get strongly-typed access to the string resources
contained in those `resw` files. This means you are left to access them similar
to this:

```cs
void DoSomething()
{
    ResourceLoader loader = new ResourceLoader();
    myThing.Content = load.GetString("ThingContent");
}
```

And hope you've typed it correctly.

In `resx` files – the predecessor of `resw` – Visual Studio would generate a
`Resources.Designer.cs` whenever you edited the file which provided strongly
typed access to resources. This wasn't continued for `resw`. Additionally that
file was committed to the repo, and not generated with every build.

Reswinator does exactly that -- generates a strongly typed accessor *at build
time*.

## Installation
Add the `Rewsinator` [nuget package](https://www.nuget.org/packages/Codevoid.Utilities.Reswinator)
to your project.

Thats it!

## Usage
Add a `resw` to your project following the [Microsoft guidelines](https://learn.microsoft.com/en-us/windows/uwp/app-resources/localize-strings-ui-manifest)
and build.

Thats it!

Now you can access you're resources this way:
```cs
void DoSomething()
{
    myThing.Content = (namespace).Resources.ThingContent;
}
```

If you have resources with `.` in them, they'll become nested classes. E.g for
a key name such as `MyThing.Text`, the class will nest like such:
```cs
(namespace).Resources.MyThing.Text
```

## Q&A

### What namespace does it generate the resource class into?
Whatever your projects `RootNamespace` property is. If you haven't specified this
the namespace will be `GeneratedResources`.

### How does it know which resw to generate for when there are multiple langauges?
It uses the projects default langauge (as specified in the project), and looks
for only `resw` files that match that language

### What if I have multiple `resw` files?
They're generated into different classes that match the file name. E.g.
`Errors.resw` becomes `<namspace>.Errors`. The default `Resources.resw` will
always be `Resources`.

Files that have `.` them will have those removed to generate the class name for
those resources. If there is an existing resource wrapper class with that name,
a number will be suffixed to that class name

### Do you support this other runtime that uses resw?
We only support Windows App SDK targets at this time. If you wish support for
others, then file an [issue](https://github.com/grork/Reswinator/issues). Pull
requests are also welcome!