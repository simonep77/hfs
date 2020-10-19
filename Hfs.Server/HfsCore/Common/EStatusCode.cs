
namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Stati della risposta
    /// </summary>
    public enum EStatusCode
    {
        Ok = 0,
        FileNotFound = 1,
        DirectoryNotFound = 2,
        AttrNotNumeric = 90,
        RootNotAllowed = 91,
        RecursiveDir = 92,
        PathAreSame = 93,
        EmptyToAddr = 94,
        InvalidPath = 95,
        DataExpected = 96,
        UnknownAction = 97,
        NoInputPath = 98,
        GenericError = 99,
    }
}
