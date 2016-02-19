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
using System.Text;
using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Parser;
using Guardtime.KSI.Utils;

namespace Guardtime.KSI.Publication
{
    /// <summary>
    ///     Publication data TLV element.
    /// </summary>
    public sealed class PublicationData : CompositeTag
    {
        private readonly ImprintTag _publicationHash;
        private readonly IntegerTag _publicationTime;

        /// <summary>
        ///     Create new publication data TLV element from TLV element.
        /// </summary>
        /// <param name="tag">TLV element</param>
        public PublicationData(ITlvTag tag) : base(tag)
        {
            if (Type != Constants.PublicationData.TagType)
            {
                throw new TlvException("Invalid publication data type(" + Type + ").");
            }

            int publicationTimeCount = 0;
            int publicationHashCount = 0;

            foreach (ITlvTag childTag in this)
            {
                switch (childTag.Type)
                {
                    case Constants.PublicationData.PublicationTimeTagType:
                        _publicationTime = new IntegerTag(childTag);
                        publicationTimeCount++;
                        break;
                    case Constants.PublicationData.PublicationHashTagType:
                        _publicationHash = new ImprintTag(childTag);
                        publicationHashCount++;
                        break;
                    default:
                        VerifyUnknownTag(childTag);
                        break;
                }
            }

            if (publicationTimeCount != 1)
            {
                throw new TlvException("Only one publication time must exist in publication data.");
            }

            if (publicationHashCount != 1)
            {
                throw new TlvException("Only one publication hash must exist in publication data.");
            }
        }

        /// <summary>
        ///     Create new publication data TLV element from publication time and publication hash.
        /// </summary>
        /// <param name="publicationTime">publication time</param>
        /// <param name="publicationHash">publication hash</param>
        public PublicationData(ulong publicationTime, DataHash publicationHash) : this(publicationTime, publicationHash, false, false)
        {
        }

        /// <summary>
        ///     Create new publication data TLV element from publication time and publication hash.
        /// </summary>
        /// <param name="publicationTime">publication time</param>
        /// <param name="publicationHash">publication hash</param>
        /// <param name="nonCritical">Is TLV element non critical</param>
        /// <param name="forward">Is TLV element forwarded</param>
        public PublicationData(ulong publicationTime, DataHash publicationHash, bool nonCritical, bool forward)
            : base(Constants.PublicationData.TagType, nonCritical, forward, new ITlvTag[]
            {
                new IntegerTag(Constants.PublicationData.PublicationTimeTagType, false, false, publicationTime),
                new ImprintTag(Constants.PublicationData.PublicationHashTagType, false, false, publicationHash)
            })
        {
            _publicationTime = (IntegerTag)this[0];
            _publicationHash = (ImprintTag)this[1];
        }

        /// <summary>
        ///     Create new publication data TLV element from publication string.
        /// </summary>
        /// <param name="publicationString">publication string</param>
        public PublicationData(string publicationString) : this(publicationString, false, false)
        {
        }

        /// <summary>
        ///     Create new publication data TLV element from publication string.
        /// </summary>
        /// <param name="publicationString">publication string</param>
        /// <param name="nonCritical">Is TLV element non critical</param>
        /// <param name="forward">Is TLV element forwarded</param>
        public PublicationData(string publicationString, bool nonCritical, bool forward)
            : base(Constants.PublicationData.TagType, nonCritical, forward, DecodePublicationString(publicationString))
        {
            _publicationTime = (IntegerTag)this[0];
            _publicationHash = (ImprintTag)this[1];
        }

        private static ITlvTag[] DecodePublicationString(string publicationString)
        {
            if (publicationString == null)
            {
                throw new TlvException("Invalid publication string: null.");
            }

            byte[] dataBytesWithCrc32 = Base32.Decode(publicationString);

            // Length needs to be at least 13 bytes (8 bytes for time plus non-empty hash imprint plus 4 bytes for crc32)
            if (dataBytesWithCrc32 == null || dataBytesWithCrc32.Length < 13)
            {
                throw new TlvException("Publication string base 32 decode failed.");
            }

            byte[] dataBytes = Util.Clone(dataBytesWithCrc32, 0, dataBytesWithCrc32.Length - 4);

            byte[] computedCrc32 = Util.EncodeUnsignedLong(Crc32.Calculate(dataBytes, 0));
            byte[] messageCrc32 = Util.Clone(dataBytesWithCrc32, dataBytesWithCrc32.Length - 4, 4);

            if (!Util.IsArrayEqual(computedCrc32, messageCrc32))
            {
                throw new TlvException("Publication string CRC 32 check failed.");
            }

            byte[] hashImprint = Util.Clone(dataBytesWithCrc32, 8, dataBytesWithCrc32.Length - 12);
            byte[] publicationTimeBytes = Util.Clone(dataBytesWithCrc32, 0, 8);

            return new ITlvTag[]
            {
                new IntegerTag(Constants.PublicationData.PublicationTimeTagType, false, false, Util.DecodeUnsignedLong(publicationTimeBytes, 0, publicationTimeBytes.Length)),
                new ImprintTag(Constants.PublicationData.PublicationHashTagType, false, false, new DataHash(hashImprint))
            };
        }

        /// <summary>
        /// Returns publication string of published data
        /// </summary>
        /// <returns></returns>
        public string GetPublicationString()
        {
            byte[] imprint = PublicationHash.Imprint;
            byte[] data = new byte[imprint.Length + 12];
            byte[] timeBytes = Util.EncodeUnsignedLong(PublicationTime);

            Array.Copy(timeBytes, 0, data, 8 - timeBytes.Length, timeBytes.Length);
            Array.Copy(imprint, 0, data, 8, imprint.Length);

            byte[] crc32 = Util.EncodeUnsignedLong(Crc32.Calculate(Util.Clone(data, 0, data.Length - 4), 0));
            Array.Copy(crc32, 0, data, data.Length - 4, crc32.Length);
            string code = Base32.Encode(data);

            StringBuilder sb = new StringBuilder();

            int index = 0;

            while (true)
            {
                if (index > 0)
                {
                    sb.Append("-");
                }

                if (index + 6 < code.Length)
                {
                    sb.Append(code.Substring(index, 6));
                    index += 6;
                }
                else
                {
                    int count = code.Length - index;
                    sb.Append(code.Substring(index, count));
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Returns publication date
        /// </summary>
        /// <returns></returns>
        public DateTime GetPublicationDate()
        {
            return Util.ConvertUnixTimeToDateTime(PublicationTime);
        }

        /// <summary>
        ///     Get publication time.
        /// </summary>
        public ulong PublicationTime => _publicationTime.Value;

        /// <summary>
        ///     Get publication hash.
        /// </summary>
        public DataHash PublicationHash => _publicationHash.Value;
    }
}