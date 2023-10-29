using MessagePack;
using Newtonsoft.Json.Linq;

namespace MoDuel.Serialization;

/// <summary>
/// Information for <see cref="JToken"/> serialization.
/// </summary>
[MessagePackObject]
public class JsonData {

    /// <summary>
    /// The data of the jtoken.
    /// </summary>
    [Key(0)]
    public string Text;

    public JsonData(JToken token) {
        Text = token.ToString();
    }

    /// <summary>
    /// Converts the <see cref="JsonData"/> bakc to a <see cref="JToken"/>.
    /// <para>Assumes <see cref="IsRoot"/> has already been checked.</para>
    /// <para>Will crash if <see cref="IsRoot"/> is not true.</para>
    /// </summary>
    public JToken Deserialize() {
        return JToken.Parse(Text);
    }

}
