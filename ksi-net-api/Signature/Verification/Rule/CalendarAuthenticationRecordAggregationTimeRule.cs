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

namespace Guardtime.KSI.Signature.Verification.Rule
{
    /// <summary>
    ///     Rule verifies that calendar authentication record aggregation time equals to calendar hash chain aggregation time.
    ///     Without calendar authentication record <see cref="VerificationResultCode.Ok" /> is returned.
    /// </summary>
    public sealed class CalendarAuthenticationRecordAggregationTimeRule : VerificationRule
    {
        /// <see cref="VerificationRule.Verify" />
        public override VerificationResult Verify(IVerificationContext context)
        {
            IKsiSignature signature = GetSignature(context);
            CalendarAuthenticationRecord calendarAuthenticationRecord = signature.CalendarAuthenticationRecord;

            if (calendarAuthenticationRecord == null)
            {
                return new VerificationResult(GetRuleName(), VerificationResultCode.Ok);
            }

            CalendarHashChain calendarHashChain = GetCalendarHashChain(signature);

            return calendarHashChain.PublicationTime != calendarAuthenticationRecord.PublicationData.PublicationTime
                ? new VerificationResult(GetRuleName(), VerificationResultCode.Fail, VerificationError.Int06)
                : new VerificationResult(GetRuleName(), VerificationResultCode.Ok);
        }
    }
}