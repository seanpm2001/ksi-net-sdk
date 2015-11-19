﻿using System.Collections.Generic;
using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Parser;

namespace Guardtime.KSI.Signature
{
    /// <summary>
    ///     Aggregation authentication record TLV element
    /// </summary>
    public sealed class AggregationAuthenticationRecord : CompositeTag
    {
        private readonly IntegerTag _aggregationTime;
        private readonly List<IntegerTag> _chainIndex = new List<IntegerTag>();
        private readonly ImprintTag _inputHash;

        /// <summary>
        ///     Create new aggregation authentication record TLV element from TLV element
        /// </summary>
        /// <param name="tag">TLV element</param>
        
        public AggregationAuthenticationRecord(ITlvTag tag) : base(tag)
        {
            if (Type != Constants.AggregationAuthenticationRecord.TagType)
            {
                throw new TlvException("Invalid aggregation authentication record type(" + Type + ").");
            }

            int aggregationTimeCount = 0;
            int inputHashCount = 0;
            int signatureDataCount = 0;

            foreach (ITlvTag childTag in this)
            {
                switch (childTag.Type)
                {
                    case Constants.AggregationAuthenticationRecord.AggregationTimeTagType:
                        _aggregationTime = new IntegerTag(childTag);
                        aggregationTimeCount++;
                        break;
                    case Constants.AggregationAuthenticationRecord.ChainIndexTagType:
                        IntegerTag chainIndexTag = new IntegerTag(childTag);
                        _chainIndex.Add(chainIndexTag);
                        break;
                    case Constants.AggregationAuthenticationRecord.InputHashTagType:
                        _inputHash = new ImprintTag(childTag);
                        inputHashCount++;
                        break;
                    case Constants.SignatureData.TagType:
                        SignatureData = new SignatureData(childTag);
                        signatureDataCount++;
                        break;
                    default:
                        VerifyUnknownTag(childTag);
                        break;
                }
            }

            if (aggregationTimeCount != 1)
            {
                throw new TlvException(
                    "Only one aggregation time must exist in aggregation authentication record.");
            }

            if (_chainIndex.Count == 0)
            {
                throw new TlvException("Chain indexes must exist in aggregation authentication record.");
            }

            if (inputHashCount != 1)
            {
                throw new TlvException(
                    "Only one input hash must exist in aggregation authentication record.");
            }

            if (signatureDataCount != 1)
            {
                throw new TlvException(
                    "Only one signature data must exist in aggregation authentication record.");
            }
        }

        /// <summary>
        ///     Get aggregation time.
        /// </summary>
        public ulong AggregationTime => _aggregationTime.Value;

        /// <summary>
        ///     Get input hash.
        /// </summary>
        public DataHash InputHash => _inputHash.Value;

        /// <summary>
        ///     Get signature data.
        /// </summary>
        public SignatureData SignatureData { get; }
    }
}