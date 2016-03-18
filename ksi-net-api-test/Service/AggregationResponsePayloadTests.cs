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

using Guardtime.KSI.Parser;
using Guardtime.KSI.Service;
using NUnit.Framework;

namespace Guardtime.KSI.Test.Service
{
    [TestFixture]
    public class AggregationResponsePayloadTests
    {
        [Test]
        public void ToStringTest()
        {
            AggregationResponsePayload tag = TestUtil.GetCompositeTag<AggregationResponsePayload>(Constants.AggregationResponsePayload.TagType, new ITlvTag[]
            {
                new IntegerTag(Constants.AggregationResponsePayload.RequestIdTagType, false, false, 2),
                new IntegerTag(Constants.KsiPduPayload.StatusTagType, false, false, 1),
                new StringTag(Constants.KsiPduPayload.ErrorMessageTagType, false, false, "Test error message."),
                new RawTag(Constants.AggregationResponsePayload.ConfigTagType, false, false, new byte[] { 0x1 }),
                new RawTag(Constants.AggregationResponsePayload.RequestAcknowledgmentTagType, false, false, new byte[] { 0x1 }),
            });

            AggregationResponsePayload tag2 = new AggregationResponsePayload(tag);

            Assert.AreEqual(tag.ToString(), tag2.ToString());
        }
    }
}