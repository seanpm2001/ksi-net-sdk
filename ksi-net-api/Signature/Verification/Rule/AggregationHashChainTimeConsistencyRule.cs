﻿using System.Collections.ObjectModel;
using Guardtime.KSI.Exceptions;
using NLog;

namespace Guardtime.KSI.Signature.Verification.Rule
{
    /// <summary>
    ///     Rule checks that aggregation hash chain times are consistent. It means that previous aggregation hash chain
    ///     aggregation time equals to current one.
    /// </summary>
    public sealed class AggregationHashChainTimeConsistencyRule : VerificationRule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


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

            ReadOnlyCollection<AggregationHashChain> aggregationHashChainCollection =
                context.Signature.GetAggregationHashChains();
            if (aggregationHashChainCollection == null)
            {
                throw new KsiVerificationException("Aggregation hash chains are missing from KSI signature.");
            }

            ulong? time = null;
            for (int i = 0; i < aggregationHashChainCollection.Count; i++)
            {
                if (time == null)
                {
                    time = aggregationHashChainCollection[i].AggregationTime;
                    continue;
                }

                if (aggregationHashChainCollection[i].AggregationTime == time)
                {
                    continue;
                }

                // TODO: Correct logging
                Logger.Error("Previous aggregation hash chain aggregation time {0} does not match current aggregation time {1}",
                    time, aggregationHashChainCollection[i].AggregationTime);
                return new VerificationResult(GetRuleName(), VerificationResultCode.Fail, VerificationError.Int02);
            }

            return new VerificationResult(GetRuleName(), VerificationResultCode.Ok);
        }
    }
}