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

using System;
using System.Collections.Generic;
using System.IO;
using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Parser;
using NLog;

namespace Guardtime.KSI.Service
{
    /// <summary>
    /// Protocol Data Unit
    /// </summary>
    public abstract class Pdu : CompositeTag
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        int _headerIndex;
        int _macIndex;

        /// <summary>
        /// List on payloads
        /// </summary>
        protected List<PduPayload> Payloads { get; } = new List<PduPayload>();

        /// <summary>
        /// Error payload
        /// </summary>
        protected PduPayload ErrorPayload { get; set; }

        /// <summary>
        ///     Get and set PDU header
        /// </summary>
        public PduHeader Header { get; private set; }

        /// <summary>
        ///     Create PDU from TLV element.
        /// </summary>
        /// <param name="tag">TLV element</param>
        protected Pdu(ITlvTag tag) : base(tag)
        {
        }

        /// <summary>
        /// Parse child tag
        /// </summary>
        protected override ITlvTag ParseChild(ITlvTag childTag)
        {
            foreach (uint tagType in Constants.AllPayloadTypes)
            {
                if (tagType == childTag.Type)
                {
                    return childTag;
                }
            }

            if (childTag.Type == Constants.PduHeader.TagType)
            {
                _headerIndex = Count;
                return Header = childTag as PduHeader ?? new PduHeader(childTag);
            }

            if (childTag.Type == Constants.Pdu.MacTagType)
            {
                _macIndex = Count;
                return Mac = GetImprintTag(childTag);
            }

            return base.ParseChild(childTag);
        }

        /// <summary>
        /// Validate the tag
        /// </summary>
        protected override void Validate(TagCounter tagCounter)
        {
            base.Validate(tagCounter);

            if (ErrorPayload == null)
            {
                if (Payloads.Count == 0)
                {
                    throw new TlvException("Payloads are missing in PDU.");
                }

                if (tagCounter[Constants.PduHeader.TagType] != 1)
                {
                    throw new TlvException("Exactly one header must exist in PDU.");
                }

                if (_headerIndex != 0)
                {
                    throw new TlvException("Header must be the first element in PDU.");
                }

                if (tagCounter[Constants.Pdu.MacTagType] != 1)
                {
                    throw new TlvException("Exactly one HMAC must exist in PDU");
                }

                if (_macIndex != Count - 1)
                {
                    throw new TlvException("HMAC must be the last element in PDU");
                }
            }
        }

        /// <summary>
        ///     Create aggregation pdu TLV element from KSI header and payload.
        /// </summary>
        /// <param name="tagType">PDU TLV tag type</param>
        /// <param name="header">PDU header</param>
        /// <param name="payload">aggregation payload</param>
        /// <param name="hmacAlgorithm">HMAC algorithm</param>
        /// <param name="key">hmac key</param>
        protected Pdu(uint tagType, PduHeader header, PduPayload payload, HashAlgorithm hmacAlgorithm, byte[] key)
            : base(tagType, false, false, new ITlvTag[] { header, payload, GetEmptyHashMacTag(hmacAlgorithm) })
        {
            SetHmacValue(hmacAlgorithm, key);
        }

        /// <summary>
        /// MAC
        /// </summary>
        public ImprintTag Mac { get; private set; }

        /// <summary>
        /// Get payload of a given type
        /// </summary>
        /// <typeparam name="T">PDU payload type</typeparam>
        /// <returns></returns>
        protected T GetPayload<T>() where T : PduPayload
        {
            foreach (PduPayload payload in Payloads)
            {
                T p = payload as T;
                if (p != null)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Get payloads of a given type
        /// </summary>
        /// <typeparam name="T">PDU payload type</typeparam>
        /// <returns></returns>
        protected IEnumerable<T> GetPayloads<T>() where T : PduPayload
        {
            foreach (PduPayload payload in Payloads)
            {
                T p = payload as T;
                if (p != null)
                {
                    yield return p;
                }
            }
        }

        /// <summary>
        /// Set HMAC tag value
        /// </summary>
        /// <param name="hmacAlgorithm"></param>
        /// <param name="key"></param>
        protected void SetHmacValue(HashAlgorithm hmacAlgorithm, byte[] key)
        {
            for (int i = 0; i < Count; i++)
            {
                ITlvTag childTag = this[i];

                if (childTag.Type == Constants.Pdu.MacTagType)
                {
                    this[i] = Mac = CreateHashMacTag(CalcHashMacValue(hmacAlgorithm, key));
                    break;
                }
            }
        }

        /// <summary>
        ///     Calculate HMAC value.
        /// </summary>
        /// <param name="hmacAlgorithm">HMAC algorithm</param>
        /// <param name="key">HMAC key</param>
        private DataHash CalcHashMacValue(HashAlgorithm hmacAlgorithm, byte[] key)
        {
            MemoryStream stream = new MemoryStream();
            using (TlvWriter writer = new TlvWriter(stream))
            {
                writer.WriteTag(this);

                return CalcHashMacValue(stream.ToArray(), hmacAlgorithm, key);
            }
        }

        /// <summary>
        ///     Calculate HMAC value.
        /// </summary>
        /// <param name="pduBytes">PDU encoded as byte array</param>
        /// <param name="hmacAlgorithm">HMAC algorithm</param>
        /// <param name="key">HMAC key</param>
        private static DataHash CalcHashMacValue(byte[] pduBytes, HashAlgorithm hmacAlgorithm, byte[] key)
        {
            byte[] target = pduBytes.Length < hmacAlgorithm.Length ? new byte[0] : new byte[pduBytes.Length - hmacAlgorithm.Length];
            Array.Copy(pduBytes, 0, target, 0, target.Length);

            IHmacHasher hasher = KsiProvider.CreateHmacHasher(hmacAlgorithm);
            return hasher.GetHash(key, target);
        }

        /// <summary>
        /// Returns HMAC tag containing given data hash value
        /// </summary>
        /// <param name="dataHash">Data hash</param>
        /// <returns></returns>
        private static ImprintTag CreateHashMacTag(DataHash dataHash)
        {
            return new ImprintTag(Constants.Pdu.MacTagType, false, false, dataHash);
        }

        /// <summary>
        /// Get HMAC tag that has hash algorithm set, but hash value is a byte array containing zeros.
        /// </summary>
        /// <param name="hmacAlgorithm">HMAC algorithm</param>
        /// <returns></returns>
        protected static ImprintTag GetEmptyHashMacTag(HashAlgorithm hmacAlgorithm)
        {
            if (hmacAlgorithm == null)
            {
                throw new TlvException("Invalid HMAC algorithm: null.");
            }

            byte[] imprintBytes = new byte[hmacAlgorithm.Length + 1];
            imprintBytes[0] = hmacAlgorithm.Id;
            return CreateHashMacTag(new DataHash(imprintBytes));
        }

        /// <summary>
        ///     Validate PDU against given MAC.
        /// </summary>
        /// <param name="pduBytes">PDU encoded as byte array</param>
        /// <param name="mac">MAC</param>
        /// <param name="key">message key</param>
        /// <returns>true if MAC is valid</returns>
        public static bool ValidateMac(byte[] pduBytes, ImprintTag mac, byte[] key)
        {
            if (pduBytes == null || mac == null)
            {
                return false;
            }

            if (pduBytes == null)
            {
                throw new ArgumentNullException(nameof(pduBytes));
            }

            if (mac == null)
            {
                throw new ArgumentNullException(nameof(mac));
            }

            if (pduBytes.Length < mac.Value.Algorithm.Length)
            {
                Logger.Warn("PDU MAC validation failed. PDU bytes array is too short to contain given MAC.");
                return false;
            }

            DataHash calculatedMac = CalcHashMacValue(pduBytes, mac.Value.Algorithm, key);

            if (!calculatedMac.Equals(mac.Value))
            {
                Logger.Warn("PDU MAC validation failed. Calculated MAC and given MAC do no match.");
                return false;
            }

            return true;
        }
    }
}