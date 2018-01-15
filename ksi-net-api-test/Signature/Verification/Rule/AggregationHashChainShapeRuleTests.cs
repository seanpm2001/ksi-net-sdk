﻿/*
 * Copyright 2013-2017 Guardtime, Inc.
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
using System.IO;
using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Signature;
using Guardtime.KSI.Signature.Verification;
using Guardtime.KSI.Signature.Verification.Rule;
using NUnit.Framework;

namespace Guardtime.KSI.Test.Signature.Verification.Rule
{
    [TestFixture]
    public class AggregationHashChainShapeRuleTests
    {
        [Test]
        public void TestMissingContext()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Argument null exception when no context
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                rule.Verify(null);
            });
            Assert.AreEqual("context", ex.ParamName);
        }

        [Test]
        public void TestContextMissingSignature()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Verification exception on missing KSI signature 
            KsiVerificationException ex = Assert.Throws<KsiVerificationException>(delegate
            {
                TestVerificationContext context = new TestVerificationContext();

                rule.Verify(context);
            });
            Assert.That(ex.Message, Does.StartWith("Invalid KSI signature in context: null"));
        }

        [Test]
        public void TestSignatureWithoutAggregationHashChain()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Verification exception on missing KSI signature aggregation hash chain 
            KsiVerificationException ex = Assert.Throws<KsiVerificationException>(delegate
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new TestKsiSignature()
                };

                rule.Verify(context);
            });
            Assert.That(ex.Message, Does.StartWith("Aggregation hash chains are missing from KSI signature"));
        }

        [Test]
        public void TestRfc3161SignatureAggregationHashChainIndex()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Check legacy signature for aggregation hash chain index consistency
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignature_Legacy_Ok), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream)
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Ok, verificationResult.ResultCode);
            }
        }

        [Test]
        public void TestSignatureAggregationHashChainIndex()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Check signature for aggregation hash chain index consistency
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignature_Ok), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream)
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Ok, verificationResult.ResultCode);
            }
        }

        [Test]
        public void TestInvalidSignatureAggregationHashChainIndex()
        {
            AggregationHashChainShapeRule rule = new AggregationHashChainShapeRule();

            // Check invalid signature for aggregation hash chain inconsistency in index
            using (FileStream stream =
                new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignature_Invalid_Aggregation_Chain_Index_Mismatch), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory(new EmptyVerificationPolicy()).Create(stream)
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Fail, verificationResult.ResultCode);
                Assert.AreEqual(VerificationError.Int10, verificationResult.VerificationError);
            }
        }
    }
}