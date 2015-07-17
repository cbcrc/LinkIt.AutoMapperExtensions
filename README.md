RC.AutoMapper
===============

AutoMapper extensions to help map *Linked Sources* to DTOs by convention.


Getting started
---------------

A *Linked Source* is a class that has a property called `Model`, which is the primary source for the DTO, and other properties for mapping associations. For example, let's say you wish to map to this DTO:

```csharp
public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public CategoryDto Category { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

And your sources are objects that came from a database, an `Article` model and a `Category` model that you fetched separately:

```csharp
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public int CategoryId { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

To use the Linked Source convention, you need to create an object like this:
```csharp
public class ArticleLinkedSource
{
    public Article Model { get; set; }
    public Category Category { get; set; }
}
```

And then map it using our `MapLinkedSource()` extension method:
```csharp
Mapper.CreateMap<ArticleLinkedSource, ArticleDto>().MapLinkedSource();
```

What this does is, for all the properties of the DTO, map them to matching properties from the linked source, or if none exists, map them to matching properties from the `Model`. It is the equivalent of this:
```csharp
Mapper.CreateMap<ArticleLinkedSource, ArticleDto>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(source => source.Model.Id))
    .ForMember(dto => dto.Title, opt => opt.MapFrom(source => source.Model.Title))
    .ForMember(dto => dto.Body, opt => opt.MapFrom(source => source.Model.Body))
    .ForMember(dto => dto.Category, opt => opt.MapFrom(source => source.Category));
```

Of course, in this example, you still need to map the other DTOs, such as `CategoryDto`. In this case, that object is quite simple, so AutoMapper's default convention works great:
```csharp
Mapper.CreateMap<Category, CategoryDto>();
```


Slightly more complex example
---------------
You have these DTOs:
```csharp
public class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public MediaDto Media { get; set; }
    public TagDto[] Tags { get; set; }
}

public class MediaDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public TagDto[] Tags { get; set; }
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

And your sources are these:
```csharp
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public int MediaId { get; set; }
    public IEnumerable<int> TagIds { get; set; }
}

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

Using our convention, you can then build these linked sources:
```csharp
public class BlogPostLinkedSource
{
    public BlogPost Model { get; set; }
    public MediaLinkedSource Media { get; set; }
    public IEnumerable<Tag> Tags { get; set; }
}

public class MediaLinkedSource
{
    public Media Model { get; set; }
    public IEnumerable<Tag> Tags { get; set; }
}
```

And then configure the mappings as such:
```csharp
Mapper.CreateMap<BlogPostLinkedSource, BlogPostDto>().MapLinkedSource();
Mapper.CreateMap<MediaLinkedSource, MediaDto>().MapLinkedSource();
Mapper.CreateMap<Tag, TagDto>();
```

This would be the equivalent of doing:
```csharp
Mapper.CreateMap<BlogPostLinkedSource, BlogPostDto>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(source => source.Model.Id))
    .ForMember(dto => dto.Title, opt => opt.MapFrom(source => source.Model.Title))
    .ForMember(dto => dto.Body, opt => opt.MapFrom(source => source.Model.Body))
    .ForMember(dto => dto.Media, opt => opt.MapFrom(source => source.Media))
    .ForMember(dto => dto.Tags, opt => opt.MapFrom(source => source.Tags));
Mapper.CreateMap<MediaLinkedSource, MediaDto>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(source => source.Model.Id))
    .ForMember(dto => dto.Title, opt => opt.MapFrom(source => source.Model.Title))
    .ForMember(dto => dto.Tags, opt => opt.MapFrom(source => source.Tags));
Mapper.CreateMap<Tag, TagDto>();
```