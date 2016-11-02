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
using Guardtime.KSI.Parser;
using Guardtime.KSI.Signature;

namespace Guardtime.KSI.Service
{
    /// <summary>
    ///     Extend response payload.
    /// </summary>
    public sealed class ExtendResponsePayload : KsiPduPayload
    {
        private readonly StringTag _errorMessage;
        private readonly IntegerTag _calendarLastTime;
        private readonly IntegerTag _requestId;
        private readonly IntegerTag _status;

        /// <summary>
        ///     Create extend response payload from TLV element.
        /// </summary>
        /// <param name="tag">TLV element</param>
        public ExtendResponsePayload(ITlvTag tag) : base(tag)
        {
            CheckTagType(Constants.ExtendResponsePayload.TagType);

            int requestIdCount = 0;
            int statusCount = 0;
            int errorMessageCount = 0;
            int calendarLastTimeCount = 0;
            int calendarHashChainCount = 0;

            for (int i = 0; i < Count; i++)
            {
                ITlvTag childTag = this[i];

                switch (childTag.Type)
                {
                    case Constants.ExtendResponsePayload.RequestIdTagType:
                        this[i] = _requestId = new IntegerTag(childTag);
                        requestIdCount++;
                        break;
                    case Constants.KsiPduPayload.StatusTagType:
                        this[i] = _status = new IntegerTag(childTag);
                        statusCount++;
                        break;
                    case Constants.KsiPduPayload.ErrorMessageTagType:
                        this[i] = _errorMessage = new StringTag(childTag);
                        errorMessageCount++;
                        break;
                    case Constants.ExtendResponsePayload.CalendarLastTimeTagType:
                        this[i] = _calendarLastTime = new IntegerTag(childTag);
                        calendarLastTimeCount++;
                        break;
                    case Constants.CalendarHashChain.TagType:
                        this[i] = CalendarHashChain = new CalendarHashChain(childTag);
                        calendarHashChainCount++;
                        break;
                    default:
                        VerifyUnknownTag(childTag);
                        break;
                }
            }

            if (requestIdCount != 1)
            {
                throw new TlvException("Exactly one request id must exist in extend response payload.");
            }

            if (statusCount != 1)
            {
                throw new TlvException("Exactly one status code must exist in extend response payload.");
            }

            if (errorMessageCount > 1)
            {
                throw new TlvException("Only one error message is allowed in extend response payload.");
            }

            if (calendarLastTimeCount > 1)
            {
                throw new TlvException("Only one calendar last time is allowed in extend response payload.");
            }

            if (_status.Value == 0 && calendarHashChainCount != 1)
            {
                throw new TlvException("Exactly one calendar hash chain must exist in extend response payload.");
            }
        }

        /// <summary>
        ///     Get calendar hash chain.
        /// </summary>
        public CalendarHashChain CalendarHashChain { get; }

        /// <summary>
        ///     Get error message if it exists.
        /// </summary>
        public string ErrorMessage => _errorMessage?.Value;

        /// <summary>
        ///     Get aggregation time of the newest calendar record the extender has
        /// </summary>
        public ulong? CalendarLastTime => _calendarLastTime?.Value;

        /// <summary>
        ///     Get request ID.
        /// </summary>
        public ulong RequestId => _requestId.Value;

        /// <summary>
        ///     Get status code.
        /// </summary>
        public ulong Status => _status.Value;
    }
}