using Newtonsoft.Json.Linq;

namespace MoDuel.Json;

/// <summary>
/// Soley use to house the <see cref="DeadToken.Instance"/> which is a <see cref="JToken"/> that is missing.
/// </summary>
public static class DeadToken {

    /// <summary>
    /// Token to use when json data is missing.
    /// </summary>
    public static readonly JToken Instance = JValue.CreateNull();


}
