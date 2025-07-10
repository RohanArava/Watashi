using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Watashi.Services;

public class RsaKeyService
{
    private readonly RsaSecurityKey _key;

    public RsaKeyService(IConfiguration config)
    {
        var privateKeyPath = config["Jwt:PrivateKeyPath"];
        if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
            throw new InvalidOperationException("RSA private key file not found.");

        using var reader = new StreamReader(privateKeyPath);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        if (keyObject is RsaPrivateCrtKeyParameters privateKeyParams)
        {
            var rsa = DotNetUtilities.ToRSA(privateKeyParams);
            _key = new RsaSecurityKey(rsa);
        }
        else
        {
            throw new InvalidCastException("Unsupported key format. Expected RSA private key.");
        }
    }

    public RsaSecurityKey GetKey() => _key;

    public JsonWebKey GetPublicJwk()
    {
        var publicKeyParams = _key.Rsa.ExportParameters(false);

        var publicRsa = RSA.Create();
        publicRsa.ImportParameters(publicKeyParams);

        var publicKey = new RsaSecurityKey(publicRsa);

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicKey);
        jwk.Use = JsonWebKeyUseNames.Sig;

        return jwk;
    }
}
