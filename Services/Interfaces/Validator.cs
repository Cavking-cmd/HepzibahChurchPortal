namespace ChurchPortal.Services.Interfaces
{
    public static class Validator
    {
        public static bool CheckNull(object? obj)
        {
            return obj == null;
        }

        public static bool CheckString(string? str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool CheckNegativeOrZero(int value)
        {
            return value <= 0;
        }

        public static bool CheckNegativeOrZero(decimal value)
        {
            return value <= 0m;
        }

        public static bool CheckNegative(int value)
        {
            return value < 0;
        }

        public static bool CheckDuplicate(bool exists)
        {
            return exists;
        }

        public static bool CheckState(bool isDeleted)
        {
            return !isDeleted;
        }

        public static DateTime AsUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        public static DateTime? AsUtc(DateTime? value)
        {
            return value.HasValue ? AsUtc(value.Value) : null;
        }
    }
}
