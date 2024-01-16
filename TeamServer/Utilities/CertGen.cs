using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HardHatCore.TeamServer.Utilities
{
    public class CertGen
    {
        public static string CertificatePath { get; set; }
        public static string CertificatePassword { get; set; } = "p@ssw0rd";
        public static X509Certificate2 cert { get; set; }

        public static async Task GenerateCert()
        {
            // Generate private-public key pair
            var rsaKey = RSA.Create(2048);

            // Describe certificate
            string subject = "CN=HardHat TeamServer";

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
                    critical: true
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

            // Export certificate to a file.
            string baseFolderPath =  Helpers.GetBaseFolderLocation();
            var DirectorySeperatorChar = Path.DirectorySeparatorChar;
            string CertificateDir = $"{baseFolderPath}{DirectorySeperatorChar}Certificates{DirectorySeperatorChar}";

            File.WriteAllBytes($"{CertificateDir}certServer.pfx", exportableCertificate.Export(X509ContentType.Pfx, passwordForCertificateProtection));
            CertificatePath = CertificateDir + $"certServer.pfx";
        }
    }
}
