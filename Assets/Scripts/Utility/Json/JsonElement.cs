using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class JsonDocument
{
    JsonElement m_root = null;

    public JsonDocument() { }
    public JsonDocument(JsonElement root) { m_root = root; }

    public void SetRoot(JsonElement root) { m_root = root; }
    public JsonElement GetRoot() { return m_root; }

    public void Read(JsonReader reader)
    {
        m_root = JsonTools.ReadNextElement(reader);
        if (m_root != null)
            m_root.Read(reader);
    }

    public void Write(JsonWriter writer)
    {
        m_root.Write(writer);
    }
}

public enum JsonType
{
    JsonObject,
    JsonArray,
    JsonString,
    JsonNumber,
}

public abstract class JsonElement
{
    JsonType m_type;
    public JsonElement(JsonType type) { m_type = type; }

    //take old index and return new index
    public abstract void Read(JsonReader reader);
    public abstract void Write(JsonWriter writer);

    public bool IsJsonObject() { return m_type == JsonType.JsonObject; }
    public JsonObject JsonObject()
    {
        if (!IsJsonObject())
            throw new InvalidCastException("Can't cast this " + m_type.ToString() + " to JsonObject");
        return this as JsonObject;
    }

    public bool IsJsonArray() { return m_type == JsonType.JsonArray; }
    public JsonArray JsonArray()
    {
        if(!IsJsonArray())
            throw new InvalidCastException("Can't cast this " + m_type.ToString() + " to JsonArray");
        return this as JsonArray;
    }

    public bool IsJsonString() { return m_type == JsonType.JsonString; }
    public JsonString JsonString()
    {
        if(!IsJsonString())
            throw new InvalidCastException("Can't cast this " + m_type.ToString() + " to JsonString");
        return this as JsonString;
    }
    public virtual string String()
    {
        var json = JsonString();

        return json.String();
    }

    public bool IsJsonNumber() { return m_type == JsonType.JsonNumber; }
    public JsonNumber JsonNumber()
    {
        if(!IsJsonNumber())
            throw new InvalidCastException("Can't cast this " + m_type.ToString() + " to JsonNumber");
        return this as JsonNumber;
    }
    public virtual int Int()
    {
        var json = JsonNumber();

        return json.Int();
    }

    public virtual float Float()
    {
        var json = JsonNumber();

        return json.Float();
    }
}

public class JsonArray : JsonElement, IEnumerable<JsonElement>
{
    List<JsonElement> m_elements = new List<JsonElement>();

    public JsonArray() : base(JsonType.JsonArray) { }

    public void Add(JsonElement element)
    {
        m_elements.Add(element);
    }

    public void Add(string value)
    {
        Add(new JsonString(value));
    }

    public void Add(int value)
    {
        Add(new JsonNumber(value));
    }

    public void Add(float value)
    {
        Add(new JsonNumber(value));
    }

    public bool Remove(JsonElement element)
    {
        return m_elements.Remove(element);
    }

    public void RemoveAt(int index)
    {
        m_elements.RemoveAt(index);
    }

    public int Size()
    {
        return m_elements.Count();
    }

    public JsonElement this[int index]
    {
        get { return m_elements[index]; }
        set { m_elements[index] = value; }
    }

    public IEnumerator<JsonElement> GetEnumerator()
    {
        return m_elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_elements.GetEnumerator();
    }

    public override void Read(JsonReader reader)
    {
        if (!reader.Read())
            return;
        do
        {
            if (reader.TokenType == JsonToken.EndArray)
                break;

            var element = JsonTools.ReadNextElement(reader, false);
            if (element == null)
                return;

            element.Read(reader);

            m_elements.Add(element);

            if (!reader.Read())
                return;

        } while (reader.TokenType != JsonToken.EndArray);
    }

    public override void Write(JsonWriter writer)
    {
        writer.WriteStartArray();
        
        foreach (var element in m_elements)
            element.Write(writer);

        writer.WriteEndArray();
    }

}

public class JsonObject : JsonElement, IEnumerable<KeyValuePair<string, JsonElement>>
{
    Dictionary<string, JsonElement> m_elements = new Dictionary<string, JsonElement>();

    public JsonObject() : base(JsonType.JsonObject) { }

    public void AddElement(string key, JsonElement element)
    {
        m_elements[key] = element;
    }

    public void AddElement(string key, string value)
    {
        AddElement(key, new JsonString(value));
    }

    public void AddElement(string key, int value)
    {
        AddElement(key, new JsonNumber(value));
    }

    public void AddElement(string key, float value)
    {
        AddElement(key, new JsonNumber(value));
    }

    public bool RemoveElement(string key)
    {
        return m_elements.Remove(key);
    }

    public bool ElementExist(string key)
    {
        return m_elements.ContainsKey(key);
    }

    public JsonElement GetElement(string key)
    {
        JsonElement elt;
        bool exist = m_elements.TryGetValue(key, out elt);
        if (exist)
            return elt;
        return null;
    }

    public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator()
    {
        return m_elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_elements.GetEnumerator();
    }

    public override void Read(JsonReader reader)
    {
        if (!reader.Read())
            return;

        do
        {
            bool readNext = true;
            string property = "";
            if (reader.TokenType == JsonToken.PropertyName)
                property = Convert.ToString(reader.Value);
            else readNext = false;

            var element = JsonTools.ReadNextElement(reader, readNext);
            if (element == null)
                return;
            element.Read(reader);

            m_elements.Add(property, element);

            if (!reader.Read())
                return;

        } while (reader.TokenType != JsonToken.EndObject);
    }

    public override void Write(JsonWriter writer)
    {
        writer.WriteStartObject();

        foreach(var element in m_elements)
        {
            writer.WritePropertyName(element.Key);
            element.Value.Write(writer);
        }

        writer.WriteEndObject();
    }

}

public class JsonString : JsonElement
{
    string m_value;

    public JsonString() : base(JsonType.JsonString) { }
    public JsonString(string value) : base(JsonType.JsonString) { m_value = value; }

    public void Set(string value) { m_value = value; }

    public override string String() { return m_value; }

    public override void Read(JsonReader reader)
    {
        if(reader.TokenType == JsonToken.String)
        {
            m_value = Convert.ToString(reader.Value);
        }
        else new ArgumentException("Invalid type with json string");
    }

    public override void Write(JsonWriter writer)
    {
        writer.WriteValue(m_value);
    }
}
public class JsonNumber : JsonElement
{
    float m_value = 0;

    public JsonNumber() : base(JsonType.JsonNumber) { }
    public JsonNumber(int value) : base(JsonType.JsonNumber) { m_value = value; }
    public JsonNumber(float value) : base(JsonType.JsonNumber) { m_value = value; }

    public void Set(int value) { m_value = value; }
    public void Set(float value) { m_value = value; }

    public override float Float() { return m_value; }
    public override int Int() { return (int)m_value; }

    public override void Read(JsonReader reader)
    {
        switch(reader.TokenType)
        {
            case JsonToken.Integer:
                m_value = Convert.ToInt32(reader.Value);
                break;
            case JsonToken.Float:
                m_value = Convert.ToSingle(reader.Value);
                break;
            default:
                throw new ArgumentException("Invalid type with json number");
        }
    }

    public override void Write(JsonWriter writer)
    {
        writer.WriteValue(m_value);
    }
}