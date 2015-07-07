﻿using System;
using Guardtime.KSI.Parser;

namespace Guardtime.KSI.Service
{
    public class AggregationResponsePayload : AggregationPduPayload
    {
        // Better name
        public const uint TagType = 0x202;
        private const uint RequestIdTagType = 0x1;
        private const uint ConfigTagType = 0x10;
        private const uint RequestAcknowledgmentTagType = 0x11;

        private readonly IntegerTag _requestId;
        private readonly IntegerTag _status;
        private readonly StringTag _errorMessage;

        // TODO: Create config
        private readonly RawTag _config;
        // TODO: Create request acknowledgement 
        private readonly RawTag _requestAcknowledgment;

        public AggregationResponsePayload(TlvTag tag) : base(tag)
        {
            for (int i = 0; i < Count; i++)
            {
                switch (this[i].Type)
                {
                    case RequestIdTagType:
                        _requestId = new IntegerTag(this[i]);
                        this[i] = _requestId;
                        break;
                    case StatusTagType:
                        _status = new IntegerTag(this[i]);
                        this[i] = _status;
                        break;
                    case ErrorMessageTagType:
                        _errorMessage = new StringTag(this[i]);
                        this[i] = _errorMessage;
                        break;
                    case ConfigTagType:
                        _config = new RawTag(this[i]);
                        this[i] = _config;
                        break;
                    case RequestAcknowledgmentTagType:
                        _requestAcknowledgment = new RawTag(this[i]);
                        this[i] = _requestAcknowledgment;
                        break;
                }
            }
        }

        protected override void CheckStructure()
        {
            throw new NotImplementedException();
        }

    }
}