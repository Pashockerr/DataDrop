using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DataDrop.Core;

public class SecurityManager
{
    /// <summary>
    /// Returns first PRIVATE, second - PUBLIC
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static (byte[] PrivateCert, byte[] PublicCert) GenerateCertificate(string password)
    {
        using RSA key = RSA.Create(3072);
        var request = new CertificateRequest(
            "CN=DataDrop",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(System.Net.IPAddress.Loopback);
        
        request.CertificateExtensions.Add(names.Build());

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature,
                critical: true));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                    new Oid("1.3.6.1.5.5.7.3.1")
                },
                critical: false));

        using X509Certificate2 certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddYears(1));
        
        
        return(
            PrivateCert:certificate.Export(X509ContentType.Pfx, password),
            PublicCert: certificate.Export(X509ContentType.Cert)
            );
    }

    public static void GenerateCertificateAndSaveToFile(string password, string privatePath, string publicPath)
    {
        var (pri, pub) = GenerateCertificate(password);
        File.WriteAllBytes(privatePath, pri);
        File.WriteAllBytes(publicPath, pub);
    }
}
