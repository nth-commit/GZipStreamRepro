using System.IO.Compression;
using System.Text;
using GalaxyCheck;

namespace GZipStreamRepro;

public class UnitTest1
{
    /// <summary>
    /// All alphanumeric strings can be (ASCII) encoded and decoded - without loss of information
    /// </summary>
    [Property]
    public Property EncodeDecodeRoundTrip()
    {
        return Gen.String().FromCharacters(Gen.CharType.Alphabetical | Gen.CharType.Numeric).ForAll(decoded0 =>
        {
            var encoded = EncodeString(decoded0);
            var decoded1 = DecodeString(encoded);
            Assert.Equal(decoded0, decoded1);
        });
    }

    /// <summary>
    /// All alphanumeric strings can be (ASCII) encoded and decoded with compression - without loss of information
    /// </summary>
    [Property]
    public Property CompressEncodeDecodeDecompressRoundTrip()
    {
        return Gen.String().FromCharacters(Gen.CharType.Alphabetical | Gen.CharType.Numeric).ForAll(decoded0 =>
        {
            var encoded = CompressAndEncodeString(decoded0);
            var decoded1 = DecodeAndDecompressString(encoded);
            Assert.Equal(decoded0, decoded1);
        });
    }

    /// <summary>
    /// Examples of encoded strings on Windows, are identical in Linux - the encoding is not the problem
    /// </summary>
    [Theory]
    [InlineData(".", "Lg==")]
    [InlineData("hello world", "aGVsbG8gd29ybGQ=")]
    public void EncodeExamples(string decoded, string encoded)
    {
        Assert.Equal(encoded, EncodeString(decoded));
    }

    /// <summary>
    /// Add compression, and these tests now fail on Linux. Seems like GZipStream differs across the platforms.
    /// </summary>
    [Theory]
    [InlineData(".", "H4sIAAAAAAAACtMDAELi1A4BAAAA")]
    [InlineData("hello world", "H4sIAAAAAAAACstIzcnJVyjPL8pJAQCFEUoNCwAAAA==")]
    public void CompressAndEncodeExamples(string decoded, string encoded)
    {
        Assert.Equal(encoded, CompressAndEncodeString(decoded));
    }

    private static string EncodeString(string str)
    {
        using var msIn = new MemoryStream(Encoding.ASCII.GetBytes(str));
        return Convert.ToBase64String(msIn.ToArray());
    }

    private static string DecodeString(string str)
    {
        using var msIn = new MemoryStream(Convert.FromBase64String(str));
        return Encoding.ASCII.GetString(msIn.ToArray());
    }

    private static string CompressAndEncodeString(string str)
    {
        using var msIn = new MemoryStream(Encoding.ASCII.GetBytes(str));
        using var msOut = new MemoryStream();
        using var gzs = new GZipStream(msOut, CompressionMode.Compress);

        msIn.CopyTo(gzs);
        gzs.Close();

        return Convert.ToBase64String(msOut.ToArray());
    }

    private static string DecodeAndDecompressString(string str)
    {
        using var msIn = new MemoryStream(Convert.FromBase64String(str));
        using var gzs = new GZipStream(msIn, CompressionMode.Decompress);
        using var msOut = new MemoryStream();

        gzs.CopyTo(msOut);

        return Encoding.ASCII.GetString(msOut.ToArray());
    }
}