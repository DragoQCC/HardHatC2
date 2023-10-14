using Bunit.Diffing;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HardHatC2Client.Utilities;

public class CertGen
{
    public static string CertificatePath { get; set; }
    public static string CertificatePassword { get; set; } = "p@ssw0rd";
    public static X509Certificate2 cert { get; set; }

    public static async Task SetCertificatePath()
    {
        // Export certificate to a file.
        var DirectorySeperatorChar = Path.DirectorySeparatorChar;
        string CertificateDir = $"{HelperFunctions.GetBaseFolderLocation()}{DirectorySeperatorChar}Certificates{DirectorySeperatorChar}";
        CertificatePath = CertificateDir + $"certClient.pfx";
    }

    public static async Task GenerateCert()
    {
        // Generate private-public key pair
        var rsaKey = RSA.Create(2048);

        // Describe certificate
        string subject = "CN=HardHat Client";

        // Create certificate request
        var certificateRequest = new CertificateRequest(
            subject,
            rsaKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        certificateRequest.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: false
            )
        );

        certificateRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                keyUsages:
                    X509KeyUsageFlags.DigitalSignature
                    | X509KeyUsageFlags.KeyEncipherment,
                
                critical: false
            )
        );

        certificateRequest.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(
                key: certificateRequest.PublicKey,
                critical: false
            )
        );

        //include that the certificate should be used for server authentication
        certificateRequest.CertificateExtensions.Add(
                       new X509EnhancedKeyUsageExtension(
                                          new OidCollection
                                          {
                                            new Oid("1.3.6.1.5.5.7.3.1") // server authentication
                                          },false));

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddIpAddress(ipAddress:System.Net.IPAddress.Any);
        certificateRequest.CertificateExtensions.Add(sanBuilder.Build());

        var expireAt = DateTimeOffset.Now.AddYears(5);

        cert = certificateRequest.CreateSelfSigned(DateTimeOffset.Now, expireAt);

        // Export certificate with private key
        var exportableCertificate = new X509Certificate2(cert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);


        //check if the operating system is windows or linux and only set friendlyName on windows
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exportableCertificate.FriendlyName = "Certificate For Client Authorization";
        }

        // Create password for certificate protection
        var passwordForCertificateProtection = new SecureString();
        foreach (var @char in "p@ssw0rd")
        {
            passwordForCertificateProtection.AppendChar(@char);
        }

           
            
        File.WriteAllBytes($"{CertificatePath}",exportableCertificate.Export(X509ContentType.Pfx,passwordForCertificateProtection));

    }

    public static async Task LoadExistingCert()
    {
        cert = new X509Certificate2(CertificatePath, CertificatePassword);
    }
}