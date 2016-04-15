<img style="float: left;" src="logo.png">
extensions for AutoMapper
===============

LinkIt extensions for [AutoMapper](http://automapper.org/) can be used to map linked sources to DTOs by conventions.

Getting started
---------------

For example, let's say you wish to map to those DTOs:

```csharp
public class MediaDto: IMultimediaContentDto{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Tag> Tags { get; set; }
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Remember our models

```csharp
public class Media
{
    public int Id { get; set; }
    public string Title { get; set; }
    public IEnumerable<int> TagIds { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

and our linked source
```csharp
public class MediaLinkedSource
{
    public Media Model { get; set; }
    public List<Tag> Tags { get; set; }
}
```

Map DTOs using our `MapLinkedSource()` extension method:
```csharp
Mapper.CreateMap<MediaLinkedSource, MediaDto>().MapLinkedSource();
```

What this does is, for all the properties of the DTO, map them to matching properties from the linked source, or if none exists, map them to matching properties from the model. It is the equivalent of this.
```csharp
Mapper.CreateMap<MediaLinkedSource, MediaDto>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(source => source.Model.Id))
    .ForMember(dto => dto.Title, opt => opt.MapFrom(source => source.Model.Title))

```

Of course, in this example, you still need to map the other DTOs, such as `TagDto`. In this case, that object is quite simple, so AutoMapper's default convention works great.
```csharp
Mapper.CreateMap<Tag, TagDto>();
```
We are done, we can leverage [AutoMapper](http://automapper.org/) to perform complex projections on our linked sources!

### Read more
- [Slightly more complex example](slightly-more-complex-example.md)
