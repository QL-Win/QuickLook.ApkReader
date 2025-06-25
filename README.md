# QuickLook.ApkReader

This is a part of the QuickLook program, migrated from [apk-reader](https://github.com/crazyants/apk-reader). 

Read apk info (package name etc..) with out appt.

```csharp
var reader = new ApkReader();
var info = reader.Read(@"D:\tmp\wx.apk");
Console.Clear();
var json = JsonConvert.SerializeObject(info,new JsonSerializerSettings
{
    Formatting = Formatting.Indented
});
Console.WriteLine(json);
Console.ReadLine();
```

