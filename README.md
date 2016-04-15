LinkIt extensions for AutoMapper
===============

LinkIt extensions for [AutoMapper](http://automapper.org/) can be used to map linked sources to DTOs by conventions.

Here is how how the [LinkIt samples](todo) can be mapped to DTOs seamlessly.

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
- [Slightly more complex example](todo)


# *** Will be a separate page ***

Slightly more complex example
---------------
In addition to the DTOs from the [getting started example](todo), you also have these DTOs:
```csharp
public class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public TagDto[] Tags { get; set; }
    public AuthorDto Author { get; set; }
    public IMultimediaContentDto MultimediaContent { get; set; }
}

public class AuthorDto {
    public string Name { get; set; }
    public string Email { get; set; }
    public ImageDto Image { get; set; }
}

public interface IMultimediaContentDto{  
}

public class ImageDto{
    public string Id { get; set; }
    public string Alt { get; set; }
    public string Url { get; set; }
}

```

Remember our models
```csharp
public class BlogPost {
    public int Id { get; set; }
    public string Title { get; set; }
    //stle: set this as a IEnumerable
    public List<string> TagIds { get; set; }
    public Author Author { get; set; }
    public MultimediaContentReference MultimediaContentRef { get; set; }
}

public class Author {
    public string Name { get; set; }
    public string Email { get; set; }
    public string ImageId { get; set; }
}

public class MultimediaContentReference {
    public string Type { get; set; }
    public string Id { get; set; }
}

public class Image {
    public string Id { get; set; }
    public string Alt { get; set; }
    public string Url { get; set; }
}
```

and our linked source
```csharp
public class BlogPostLinkedSource : ILinkedSource<BlogPost> {
    public BlogPost Model { get; set; }
    public List<Tag> Tags { get; set; }
    public AuthorLinkedSource Author { get; set; }
    public object MultimediaContent { get; set; }
}

public class AuthorLinkedSource : ILinkedSource<Author> {
    public Author Model { get; set; }
    public Image Image { get; set; }
}
```

And then configure the mappings as such:
```csharp
Mapper.CreateMap<BlogPostLinkedSource, BlogPostDto>().MapLinkedSource();
Mapper.CreateMap<AuthorLinkedSource, AuthorDto>().MapLinkedSource();
```
This would be the equivalent of doing:
```csharp
Mapper.CreateMap<BlogPostLinkedSource, BlogPostDto>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(source => source.Model.Id))
    .ForMember(dto => dto.Title, opt => opt.MapFrom(source => source.Model.Title))
Mapper.CreateMap<AuthorLinkedSource, AuthorDto>()
    .ForMember(dto => dto.Name, opt => opt.MapFrom(source => source.Model.Name))
    .ForMember(dto => dto.Email, opt => opt.MapFrom(source => source.Model.Email))
```

You still need to map the other DTOs
```csharp
Mapper.CreateMap<Image, ImageDto>();
```

And finaly you need to handle the polymorphic property `BlogPostLinkedSource/MultimediaContent`, but this is a built-in feature of AutoMapper.
```csharp
Mapper.CreateMap<object, Dtos.v1.Interfaces.IMultimediaContentDto>()
    .Include<Image, ImageDto>()
    .Include<MediaLinkedSource, MediaDto>()
```

