using System;
using System.Text.RegularExpressions;
using ClickstreamAnalytics.Util;

namespace ClickstreamAnalytics.Provider
{
    public static class EventChecker
    {
        public static EventError CheckEventName(string eventName)
        {
            if (!IsValidName(eventName))
            {
                return new EventError
                {
                    ErrorCode = Event.ErrorCode.EventNameInvalid,
                    ErrorMessage =
                        "Event name can only contains uppercase and lowercase letters, underscores, number," +
                        $" and is not start with a number. event name: {eventName}."
                };
            }

            if (eventName.Length > Event.Limit.MaxEventTypeLength)
            {
                return new EventError
                {
                    ErrorCode = Event.ErrorCode.EventNameLengthExceed,
                    ErrorMessage = "Event name is too long, the max event type length is" +
                                   $" {Event.Limit.MaxEventTypeLength} characters. event name: {eventName}."
                };
            }

            return new EventError
            {
                ErrorCode = Event.ErrorCode.NoError
            };
        }

        private static bool IsValidName(string name)
        {
            if (name == null) return false;
            const string regexPattern = "^(?![0-9])[0-9a-zA-Z_]+$";
            return Regex.IsMatch(name, regexPattern);
        }

        private static bool IsValidValueType(object value)
        {
            return value is int or uint or byte or sbyte or short or ushort or long or ulong or float
                or double or decimal or string or bool;
        }

        public static EventError CheckAttributes(int currentNumber, string key, object value)
        {
            EventError error = null;
            string errorMsg = null;

            if (currentNumber >= Event.Limit.MaxNumOfAttributes)
            {
                errorMsg =
                    $"reached the max number of attributes limit {Event.Limit.MaxNumOfAttributes}." +
                    $" and the attribute: {key} will not be recorded";
                var errorString = $"attribute name: {key}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.AttributeSizeExceed
                };
            }
            else if (key.Length > Event.Limit.MaxLengthOfName)
            {
                errorMsg =
                    $"attribute : {key}, reached the max length of attributes name limit ({Event.Limit.MaxLengthOfName})." +
                    $" current length is: ({key.Length}) and the attribute will not be recorded";
                var errorString = $"attribute name length is: ({key.Length}) name is: {key}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.AttributeNameLengthExceed
                };
            }
            else if (!IsValidName(key))
            {
                errorMsg =
                    $"attribute : {key}, was not valid, attribute name can only contains uppercase and lowercase letters," +
                    " underscores, number, and is not start with a number, so the attribute will not be recorded";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(key),
                    ErrorCode = Event.ErrorCode.AttributeNameInvalid
                };
            }
            else if (!IsValidValueType(value))
            {
                errorMsg =
                    $"attribute : {key}'s value was not valid type, attribute value can only be int, uint, long, ulong, byte, " +
                    "sbyte, short, ushort, float, double, decimal, string and bool, so the attribute will not be recorded";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(key),
                    ErrorCode = Event.ErrorCode.AttributeValueTypeInvalid
                };
            }
            else if (value.ToString().Length > Event.Limit.MaxLengthOfValue)
            {
                errorMsg =
                    $"attribute : {key}, reached the max length of attributes value limit ({Event.Limit.MaxLengthOfValue})." +
                    $" current length is: ({value.ToString().Length}). and the attribute will not be recorded, attribute value: {value}.";
                var errorString = $"attribute name: {key}, attribute value: {value}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.AttributeValueLengthExceed
                };
            }

            if (error == null)
                return new EventError
                {
                    ErrorCode = Event.ErrorCode.NoError
                };
            ClickstreamLog.Warn(errorMsg);
            return error;
        }

        private static string GetLimitString(string str)
        {
            return str[..Math.Min(str.Length, Event.Limit.MaxLengthOfErrorValue)];
        }

        public static EventError CheckUserAttribute(int currentNumber, string key, object value)
        {
            EventError error = null;
            string errorMsg = null;

            if (currentNumber >= Event.Limit.MaxNumOfUserAttributes)
            {
                errorMsg =
                    $"reached the max number of user attributes limit ({Event.Limit.MaxNumOfUserAttributes})." +
                    $" and the user attribute: {key} will not be recorded";
                var errorString = $"attribute name:{key}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.UserAttributeSizeExceed
                };
            }
            else if (key.Length > Event.Limit.MaxLengthOfName)
            {
                errorMsg =
                    $"user attribute : {key}, reached the max length of attributes name limit ({Event.Limit.MaxLengthOfName})." +
                    $" current length is: ({key.Length}) and the attribute will not be recorded";
                var errorString = $"user attribute name length is: ({key.Length}) name is: {key}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.UserAttributeNameLengthExceed
                };
            }
            else if (!IsValidName(key))
            {
                errorMsg =
                    $"user attribute : {key}, was not valid, user attribute name can only contains uppercase and lowercase letters," +
                    " underscores, number, and is not start with a number. so the attribute will not be recorded";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(key),
                    ErrorCode = Event.ErrorCode.UserAttributeNameInvalid
                };
            }
            else if (!IsValidValueType(value))
            {
                errorMsg =
                    $"user attribute : {key}'s value was not valid type, attribute value can only be int, uint, long, ulong, byte, " +
                    "sbyte, short, ushort, float, double, decimal, string and bool, so the attribute will not be recorded";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(key),
                    ErrorCode = Event.ErrorCode.UserAttributeValueTypeInvalid
                };
            }
            else if (value.ToString().Length > Event.Limit.MaxLengthOfUserValue)
            {
                errorMsg =
                    $"user attribute : {key}, reached the max length of attributes value limit ({Event.Limit.MaxLengthOfUserValue})." +
                    $" current length is: ({value.ToString().Length}). and the attribute will not be recorded, attribute value: {value}.";
                var errorString = $"attribute name: {key}, attribute value: {value}.";
                error = new EventError
                {
                    ErrorMessage = GetLimitString(errorString),
                    ErrorCode = Event.ErrorCode.UserAttributeValueLengthExceed
                };
            }

            if (error == null)
                return new EventError
                {
                    ErrorCode = Event.ErrorCode.NoError
                };
            ClickstreamLog.Warn(errorMsg);
            return error;
        }
    }

    public class EventError
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}