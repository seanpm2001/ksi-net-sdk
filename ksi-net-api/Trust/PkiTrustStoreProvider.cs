﻿/*
 * Copyright 2013-2016 Guardtime, Inc.
 *
 * This file is part of the Guardtime client SDK.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *     http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES, CONDITIONS, OR OTHER LICENSES OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 * "Guardtime" and "KSI" are trademarks or registered trademarks of
 * Guardtime, Inc., and no license to trademarks is granted; Guardtime
 * reserves and retains all trademark rights.
 */

using System;
using System.Security.Cryptography.X509Certificates;
using Guardtime.KSI.Crypto;
using Guardtime.KSI.Exceptions;

namespace Guardtime.KSI.Trust
{
    /// <summary>
    ///     PKI trust store provider.
    /// </summary>
    public class PkiTrustStoreProvider : IPkiTrustProvider
    {
        private readonly X509Store _trustStore;
        private readonly ICertificateSubjectRdnSelector _certificateRdnSelector;

        /// <summary>
        /// Create PKI trust store provider instance.
        /// </summary>
        /// <param name="trustStore">trust anchors</param>
        /// <param name="certificateRdnSelector">certificate subject rdn selector</param>
        public PkiTrustStoreProvider(X509Store trustStore, ICertificateSubjectRdnSelector certificateRdnSelector)
        {
            if (certificateRdnSelector == null)
            {
                throw new ArgumentNullException(nameof(certificateRdnSelector));
            }

            _trustStore = trustStore;
            _certificateRdnSelector = certificateRdnSelector;
        }

        /// <summary>
        ///     Verify bytes with x509 signature.
        /// </summary>
        /// <param name="signedBytes"></param>
        /// <param name="signatureBytes"></param>
        public void Verify(byte[] signedBytes, byte[] signatureBytes)
        {
            if (signedBytes == null)
            {
                throw new PkiVerificationErrorException("Invalid signed bytes: null.");
            }

            if (signatureBytes == null)
            {
                throw new PkiVerificationErrorException("Invalid signature bytes: null.");
            }

            ICryptoSignatureVerifier verifier = KsiProvider.CreatePkcs7CryptoSignatureVerifier(_trustStore, _certificateRdnSelector);
            verifier.Verify(signedBytes, signatureBytes, null);
        }
    }
}