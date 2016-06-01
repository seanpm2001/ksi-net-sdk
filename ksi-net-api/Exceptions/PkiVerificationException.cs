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

namespace Guardtime.KSI.Exceptions
{
    /// <summary>
    ///     PKI signature verification exception.
    /// </summary>
    [Serializable]
    public class PkiVerificationException : KsiException
    {
        /// <summary>
        ///     Create new PKI signature verification exception with message.
        /// </summary>
        /// <param name="message">exception message</param>
        public PkiVerificationException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Create new PKI signature verification exception with message and inner exception.
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public PkiVerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}