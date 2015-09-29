﻿using Guardtime.KSI.Exceptions;

namespace Guardtime.KSI.Signature.Verification.Rule
{
    /// <summary>
    ///     Rule checks that user has provided a publication.
    /// </summary>
    public sealed class UserProvidedPublicationExistenceRule : VerificationRule
    {
        /// <summary>
        /// Rule name.
        /// </summary>
        public const string RuleName = "UserProvidedPublicationExistenceRule";

        /// <see cref="VerificationRule.Verify" />
        /// <exception cref="KsiException">thrown if verification context is missing</exception>
        public override VerificationResult Verify(IVerificationContext context)
        {
            if (context == null)
            {
                throw new KsiException("Invalid verification context: null.");
            }

            return context.UserPublication == null ? new VerificationResult(RuleName, VerificationResultCode.Na, VerificationError.Gen02) : new VerificationResult(RuleName, VerificationResultCode.Ok);
        }
    }
}