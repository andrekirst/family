namespace Api.Childs;

public static class ErrorCodes
{
    private static class Common
    {
        public const string Required = "REQUIRED";
        public const string LengthExceeded = "LENGTHEXCEEDED";
    }

    public static class Child
    {
        private const string Prefix = "CHILD";

        public static class FirstName
        {
            private const string Subject = "FIRSTNAME";

            public const string Required = $"{Prefix}_{Subject}_{Common.Required}";
            public const string LengthExceeded = $"{Prefix}_{Subject}_{Common.LengthExceeded}";
        }
    }
}