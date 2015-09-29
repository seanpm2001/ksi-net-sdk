﻿using Guardtime.KSI.Exceptions;

namespace Guardtime.KSI.Signature.Verification.Rule
{
    /// <summary>
    ///     Rule for checking if KSI signature contains calendar hash chain.
    ///     Used for key-based and publication-based verification policies.
    /// </summary>
    public sealed class CalendarHashChainExistenceRule : VerificationRule
    {
        /// <summary>
        /// Rule name.
        /// </summary>
        public const string RuleName = "CalendarHashChainExistenceRule";

        /// <see cref="VerificationRule.Verify" />
        /// <exception cref="KsiException">thrown if verification context is missing</exception>
        /// <exception cref="KsiVerificationException">thrown if verification cannot occur</exception>
        public override VerificationResult Verify(IVerificationContext context)
        {
            if (context == null)
            {
                throw new KsiException("Invalid verification context: null.");
            }

            if (context.Signature == null)
            {
                throw new KsiVerificationException("Invalid KSI signature in context: null.");
            }

            return context.Signature.CalendarHashChain == null ? new VerificationResult(RuleName, VerificationResultCode.Na, VerificationError.Gen02) : new VerificationResult(RuleName, VerificationResultCode.Ok);
        }
    }
}