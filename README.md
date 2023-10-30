<img src="https://github.com/jchristn/SerializationHelper/blob/main/Assets/logo.png?raw=true" data-canonical-src="https://github.com/jchristn/SerializationHelper/blob/main/Assets/logo.png?raw=true" width="128" height="128" />

# SerializationHelper

SerializationHelper is a simple wrapper around ```System.Text.Json``` to overcome some of the common challenges developers face when using Microsoft's JSON library. 

[![NuGet Version](https://img.shields.io/nuget/v/SerializationHelper.svg?style=flat)](https://www.nuget.org/packages/SerializationHelper/) [![NuGet](https://img.shields.io/nuget/dt/SerializationHelper.svg)](https://www.nuget.org/packages/SerializationHelper) 

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github. We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## Overview

This project was built to provide a simple interface over ```System.Text.Json```.  Let's face it.  Migrating from the wonderful ```Newtonsoft.Json``` package wasn't as easy as anyone expected.  Microsoft's implementation is strong but has strong opinions and doesn't, as of the creation of this library, provide the same level of out-of-the-box experience as the Newtonsoft implementation.

This library is my attempt at trying to make the Microsoft implementation behave in a manner consistent to what I experienced while using the Newtonsoft implementation.

## New in v1.0.x

- Initial release

## Example Project

Refer to the ```Test``` project for exercising the library.

```csharp
using SerializationHelper;

public class Person
{
  public int Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
}

Person p1 = new Person { Id = 10, FirstName = "Joe", LastName = "Smith" };
Console.WriteLine(Serializer.SerializeJson(p1, false)); // false = not pretty print
// {"Id":10,"FirstName":"Joe","LastName":"Smith"}

Person p2 = Serializer.DeserializeJson<Person>("{\"Id\":10,\"FirstName\":\"Joe\",\"LastName\":\"Smith\"}");
Console.WriteLine(p2.Id + ": " + p2.FirstName + " " + p2.LastName);
// 10: Joe Smith
```

## Version History

Refer to CHANGELOG.md for version history.
