using System.Text.Json;
using System.Text.Json.Serialization;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.API.Converters;

/// <summary>
/// Custom JSON converter for CategoryTreeDto that handles hierarchical serialization
/// </summary>
public class CategoryTreeDtoConverter : JsonConverter<CategoryTreeDto>
{
    public override CategoryTreeDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        int id = 0;
        string name = string.Empty;
        string description = string.Empty;
        int? parentCategoryId = null;
        List<CategoryTreeDto> children = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CategoryTreeDto(id, name, description, parentCategoryId, children);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
            }

            string propertyName = reader.GetString() ?? string.Empty;
            reader.Read();

            switch (propertyName.ToLowerInvariant())
            {
                case "id":
                    id = reader.GetInt32();
                    break;
                case "name":
                    name = reader.GetString() ?? string.Empty;
                    break;
                case "description":
                    description = reader.GetString() ?? string.Empty;
                    break;
                case "parentcategoryid":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        parentCategoryId = null;
                    }
                    else
                    {
                        parentCategoryId = reader.GetInt32();
                    }
                    break;
                case "children":
                    children = ReadChildren(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, CategoryTreeDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        if (value.ParentCategoryId.HasValue)
        {
            writer.WriteNumber("parentCategoryId", value.ParentCategoryId.Value);
        }
        else
        {
            writer.WriteNull("parentCategoryId");
        }

        writer.WritePropertyName("children");
        WriteChildren(writer, value.Children, options);

        writer.WriteEndObject();
    }

    private List<CategoryTreeDto> ReadChildren(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for children");
        }

        var children = new List<CategoryTreeDto>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return children;
            }

            var child = Read(ref reader, typeof(CategoryTreeDto), options);
            if (child is not null)
            {
                children.Add(child);
            }
        }

        throw new JsonException("Unexpected end of array");
    }

    private void WriteChildren(Utf8JsonWriter writer, List<CategoryTreeDto> children, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var child in children)
        {
            Write(writer, child, options);
        }

        writer.WriteEndArray();
    }
}
