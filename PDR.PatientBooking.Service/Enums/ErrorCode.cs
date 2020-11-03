using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PDR.PatientBooking.Service.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorCode
    {
        Unknown = 0,
        NotFound = 404,
        Unauthorized = 401,
        BadRequest = 400,
        Conflict = 409,
        PreconditionRequired = 428, // when meanwhile a third party has modified the state
        ServerError = 500,
    }
}