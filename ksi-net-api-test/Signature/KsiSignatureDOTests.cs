﻿using System.IO;
using Guardtime.KSI.Exceptions;
using NUnit.Framework;

namespace Guardtime.KSI.Signature
{
    [TestFixture]
    public class KsiSignatureDoTests
    {
        [Test]
        public void TestKsiSignatureDoOk()
        {
            IKsiSignature signature = GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Ok);
            Assert.NotNull(signature.CalendarHashChain, "Calendar hash chain cannot be null");
        }

        [Test]
        public void TestKsiSignatureDoOkMissingCalendarHashChain()
        {
            IKsiSignature signature = GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Ok_Missing_Calendar_Hash_Chain);
            Assert.Null(signature.CalendarHashChain, "Calendar hash chain must be null");
        }

        [Test]
        public void TestKsiSignatureDoOkMissingPublicationRecord()
        {
            IKsiSignature signature = GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Ok_Missing_Publication_Record_And_Calendar_Authentication_Record);
            Assert.Null(signature.PublicationRecord, "Publication record must be null");
            Assert.Null(signature.CalendarAuthenticationRecord, "Calendar authentication record must be null");
        }

        [Test]
        public void TestLegacyKsiSignatureDoOk()
        {
            IKsiSignature signature = GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Legacy_Ok);
            Assert.IsTrue(signature.IsRfc3161Signature, "RFC3161 tag must exist");
        }

        [Test]
        public void TestKsiSignatureDoInvalidType()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Type);
            }, "Invalid signature type: 2201");
        }

        [Test]
        public void TestKsiSignatureDoInvalidContainsPublicationRecordAndCalendarAuthenticationRecord()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Contain_Publication_Record_And_Calendar_Authentication_Record);
            }, "Only one from publication record or calendar authentication record is allowed in signature data object");
        }

        [Test]
        public void TestKsiSignatureDoInvalidExtraTag()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Extra_Tag);
            }, "Invalid tag");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMissingAggregationHashChain()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Missing_Aggregation_Hash_Chain);
            }, "Aggregation hash chains must exist in signature data object");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMissingCalendarHashChain()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Missing_Calendar_Hash_Chain);
            }, "No publication record or calendar authentication record is allowed in signature data object if there is no calendar hash chain");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMultipleCalendarAuthenticationRecords()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Multiple_Calendar_Authentication_Records);
            }, "Only one from publication record or calendar authentication record is allowed in signature data object");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMultipleCalendarHashChain()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Multiple_Calendar_Hash_Chains);
            }, "Only one calendar hash chain is allowed in signature data object");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMultiplePublicationRecords()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Multiple_Publication_Records);
            }, "Only one from publication record or calendar authentication record is allowed in signature data object");
        }

        [Test]
        public void TestKsiSignatureDoInvalidMultipleRfc3161Records()
        {
            Assert.Throws<TlvException>(delegate
            {
                GetKsiSignatureDoFromFile(Properties.Resources.KsiSignatureDo_Invalid_Multiple_Rfc_3161_Records);
            }, "Only one RFC 3161 record is allowed in signature data object");
        }

        // TODO: Multiple aggregation authentication record test is missing

        private static IKsiSignature GetKsiSignatureDoFromFile(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                return new KsiSignatureFactory().Create(stream);
            }
        }
    }
}