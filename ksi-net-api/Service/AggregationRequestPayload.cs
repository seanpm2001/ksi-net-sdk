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

using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Parser;

namespace Guardtime.KSI.Service
{
    /// <summary>
    ///     Aggregation request payload.
    /// </summary>
    public sealed class AggregationRequestPayload : KsiPduPayload
    {
        private ImprintTag _requestHash;
        private IntegerTag _requestId;
        private IntegerTag _requestLevel;

        /// <summary>
        ///     Create aggregation request payload from TLV element.
        /// </summary>
        /// <param name="tag">TLV element</param>
        public AggregationRequestPayload(ITlvTag tag) : base(tag)
        {
        }

        /// <summary>
        /// Validate the tag
        /// </summary>
        protected override void Validate()
        {
            CheckTagType(Constants.AggregationRequestPayload.TagType);

            base.Validate();

            int requestIdCount = 0;
            int requestHashCount = 0;
            int requestLevelCount = 0;

            for (int i = 0; i < Count; i++)
            {
                ITlvTag childTag = this[i];

                switch (childTag.Type)
                {
                    case Constants.KsiPduPayload.RequestIdTagType:
                        this[i] = _requestId = new IntegerTag(childTag);
                        requestIdCount++;
                        break;
                    case Constants.AggregationRequestPayload.RequestHashTagType:
                        this[i] = _requestHash = new ImprintTag(childTag);
                        requestHashCount++;
                        break;
                    case Constants.AggregationRequestPayload.RequestLevelTagType:
                        this[i] = _requestLevel = new IntegerTag(childTag);
                        requestLevelCount++;
                        break;
                    default:
                        VerifyUnknownTag(childTag);
                        break;
                }
            }

            if (requestIdCount != 1)
            {
                throw new TlvException("Exactly one request id must exist in aggregation request payload.");
            }

            if (requestHashCount != 1)
            {
                throw new TlvException("Exactly one request hash must exist in aggregation request payload.");
            }

            if (requestLevelCount > 1)
            {
                throw new TlvException("Only one request level is allowed in aggregation request payload.");
            }
        }

        /// <summary>
        ///     Create aggregation request payload from data hash.
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="hash">data hash</param>
        public AggregationRequestPayload(ulong requestId, DataHash hash) : base(Constants.AggregationRequestPayload.TagType, false, false, new ITlvTag[]
        {
            new IntegerTag(Constants.KsiPduPayload.RequestIdTagType, false, false, requestId),
            new ImprintTag(Constants.AggregationRequestPayload.RequestHashTagType, false, false, hash)
        })
        {
            _requestId = (IntegerTag)this[0];
            _requestHash = (ImprintTag)this[1];
        }

        /// <summary>
        ///     Create aggregation request payload from data hash.
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="hash">data hash</param>
        /// <param name="level">the level value of the aggregation tree node</param>
        public AggregationRequestPayload(ulong requestId, DataHash hash, uint level) : base(Constants.AggregationRequestPayload.TagType, false, false, new ITlvTag[]
        {
            new IntegerTag(Constants.KsiPduPayload.RequestIdTagType, false, false, requestId),
            new ImprintTag(Constants.AggregationRequestPayload.RequestHashTagType, false, false, hash),
            new IntegerTag(Constants.AggregationRequestPayload.RequestLevelTagType, false, false, level)
        })
        {
            _requestId = (IntegerTag)this[0];
            _requestHash = (ImprintTag)this[1];
            _requestLevel = (IntegerTag)this[2];
        }

        /// <summary>
        ///     Get request hash.
        /// </summary>
        public DataHash RequestHash => _requestHash.Value;

        /// <summary>
        ///     Get request ID.
        /// </summary>
        public ulong RequestId => _requestId.Value;

        /// <summary>
        ///     Get request level if it exists.
        /// </summary>
        public ulong? RequestLevel => _requestLevel?.Value;
    }
}