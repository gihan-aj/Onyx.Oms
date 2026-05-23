namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface ICryptoService
    {
        string Encrypt(string planeText);
        string Decrypt(string cipherText);
    }
}
