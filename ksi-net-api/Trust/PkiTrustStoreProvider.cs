﻿using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Guardtime.KSI.Trust
{
    public class PkiTrustStoreProvider : IPkiTrustProvider
    {

        public string Name
        {
            // TODO: Correct return
            get { return ""; } 
        }

        public void Verify(byte[] signedBytes, byte[] x509SignatureBytes)
        {
            if (x509SignatureBytes == null)
            {
                throw new ArgumentNullException("x509SignatureBytes");
            }
            
            SignedCms signedCms = new SignedCms(new ContentInfo(signedBytes), true);
            signedCms.Decode(x509SignatureBytes);
            signedCms.CheckSignature(false);

            Console.WriteLine(signedCms.SignerInfos[0].Certificate.GetNameInfo(X509NameType.EmailName, false));
        }
    }
}
