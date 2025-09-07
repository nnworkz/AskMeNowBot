using System.Globalization;

using AskMeNowBot.Exceptions;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

using Scalar = YamlDotNet.Core.Events.Scalar;

namespace AskMeNowBot.Configuration.Converter;

public class StringToNumberConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(int) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(double);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer serializer)
    {
        var value = parser.Consume<Scalar>().Value;
        var culture = CultureInfo.InvariantCulture;

        return type switch
        {
            not null when type == typeof(int) && int.TryParse(value, out var i) => i,
            not null when type == typeof(float) && float.TryParse(value, NumberStyles.Any, culture, out var f) => f,
            not null when type == typeof(double) && double.TryParse(value, NumberStyles.Any, culture, out var d) => d,
            not null when type == typeof(double) && double.TryParse(value, NumberStyles.Any, culture, out var m) => m,
            _ => throw new FailedConvertException()
        };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var valueString = value?.ToString();
        
        if (valueString == null)
        {
            throw new NullValueException();
        }
        
        emitter.Emit(new Scalar(null, null, valueString, ScalarStyle.Any, true, false));
    }
}
