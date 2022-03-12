using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.InReach
{
    public enum GarminErrorCodes
    {
        [Description("An unexpected error occurred (returned when the error does not fall into any other category).")]
        InternalError = 1,
        [Description("The server is under heavy usage and cannot satisfy your request. Waiting a few seconds and trying again should generally resolve the issue.")]
        TooManyRequestsError = 2,
        [Description("Invalid username or password.")]
        AuthenticationError = 3,
        [Description("The specified IMEI does not belong to the tenant.")]
        UnknownDeviceError = 4,
        [Description("The message length is invalid. A text message may not be empty and has a maximum possible length of 160.When sending a reference point with a non - empty label, the length of the location’s label counts towards this limit.")]
        InvalidMessageError = 5,
        [Description("The message timestamp is invalid. The timestamp of the message must be on or after Jan 1, 2011, and cannot be in the future.")]
        InvalidTimestampError = 6,
        [Description("The message sender is invalid. The sender must be a valid phone number or email address.")]
        InvalidSenderError = 7,
        [Description("The location's altitude is invalid. The height above the ellipsoid expressed in meters and must be between - 1, 000 and + 18, 000 meters inclusive.")]
        InvalidAltitudeError = 8,
        [Description("The location's speed is invalid. The speed is expressed in km / h and must be between 0 and 1, 854km / h inclusive.")]
        InvalidSpeedError = 9,
        [Description("The location's course is invalid. The course must be between - 360° and + 360° inclusive.")]
        InvalidCourseError = 10,
        [Description("The location's position is invalid. The latitude must be between - 90° and + 90° and the longitude between - 180° and + 180°.")]
        InvalidPositionError = 11,
        [Description("The tracking interval is invalid. The tracking interval is specified in seconds and must be between 30 and 65535 seconds.")]
        InvalidIntervalError = 12,
        [Description("The location’s type is invalid. The location type must be 0(reference point) or 1(GPS location).")]
        InvalidLocationTypeError = 13,
        [Description("The location’s label is invalid. The optional label may only be supplied when a reference point is sent, and its length is limited to 160 characters minus the length of the message.")]
        InvalidLabelError = 14,
        [Description("The account’s emergencies are not handled by the account owner.")]
        IllegalEmergencyActionError = 15,
        [Description("The binary type is invalid. The binary type must be 0(Encrypted Binary), 1(Generic Binary), or 2(Encrypted Pinpoint).")]
        InvalidBinaryType = 16,
        [Description("The binary payload is invalid. The binary payload must be base64 encoded and no greater than 268 bytes.")]
        InvalidPayloadError = 17
    }
}
