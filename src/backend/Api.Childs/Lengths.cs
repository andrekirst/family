namespace Api.Childs;

public static class Lengths
{
    public static class Child
    {
        public static class FirstName
        {
            public const int MaximumLength = 512;
        }

        public static class LastName
        {
            public const int MaximumLength = 512;
        }

        public static class CountryAlpha3Code
        {
            public const int Length = 3;
        }

        public static class City
        {
            public const int MaximumLength = 512;
        }
    }
}