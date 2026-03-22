namespace MyDashboard.Helpers;

public static class RoomKeyGenerator
{
    private const string Chars = "qwertyuiop[]asdfghjkl;zxcvbnm,./QWERTYUIOP{}ASDFGHJKL:ZXCVBNM1234567890";

    public static string Generate(int length = 10)
    {
        var random = new Random();

        return new string(Enumerable.Range(0, length)
            .Select(_ => Chars[random.Next(Chars.Length)])
            .ToArray());
    }
}