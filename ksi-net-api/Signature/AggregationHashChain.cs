﻿using System;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Parser;
using System.Collections.Generic;

namespace Guardtime.KSI.Signature
{
    public class AggregationHashChain : CompositeTag
    {
        protected IntegerTag AggregationTime;

        protected List<IntegerTag> ChainIndex;

        protected TlvTag InputData;

        protected ImprintTag InputHash;

        protected IntegerTag AggrAlgorithmId;

        protected List<Link> Chain;

        // the hash algorithm identified by aggrAlgorithmId
        protected HashAlgorithm AggrAlgorithm;

        public AggregationHashChain(TlvTag tag) : base(tag)
        {
            for (int i = 0; i < this.Count; i++)
            {
                switch (this[i].Type)
                {
                    case 0x2:
                        AggregationTime = new IntegerTag(this[i]);
                        this[i] = AggregationTime;
                        break;
                    case 0x3:
                        if (ChainIndex == null)
                        {
                            ChainIndex = new List<IntegerTag>();
                        }

                        IntegerTag chainIndexTag = new IntegerTag(this[i]);
                        ChainIndex.Add(chainIndexTag);
                        this[i] = chainIndexTag;
                        break;
                    case 0x4:
                        InputData = new RawTag(this[i]);
                        this[i] = InputData;
                        break;
                    case 0x5:
                        InputHash = new ImprintTag(this[i]);
                        this[i] = InputHash;
                        break;
                    case 0x6:
                        AggrAlgorithmId = new IntegerTag(this[i]);
                        this[i] = AggrAlgorithmId;
                        break;
                    case 0x7:
                    case 0x8:
                        if (Chain == null)
                        {
                            Chain = new List<Link>();
                        }

                        Link linkTag = new Link(this[i], (LinkDirection)Enum.ToObject(typeof(LinkDirection), (byte)this[i].Type));
                        Chain.Add(linkTag);
                        this[i] = linkTag;
                        break;
                }
            }
        }

        protected class Link : CompositeTag
        {

            protected IntegerTag LevelCorrection;

            protected ImprintTag SiblingHash;

            protected ImprintTag MetaHash;

            private MetaData _metaData;

            private LinkDirection _direction;

            // the client ID extracted from metaHash
            protected string MetaHashId;


            public Link(TlvTag tag, LinkDirection direction) : base(tag)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    switch (this[i].Type)
                    {
                        case 0x1:
                            LevelCorrection = new IntegerTag(this[i]);
                            this[i] = LevelCorrection;
                            break;
                        case 0x2:
                            SiblingHash = new ImprintTag(this[i]);
                            this[i] = SiblingHash;
                            break;
                        case 0x3:
                            MetaHash = new ImprintTag(this[i]);
                            this[i] = MetaHash;
                            break;
                        case 0x4:
                            _metaData = new MetaData(this[i]);
                            this[i] = _metaData;
                            break;
                    }
                }
            }

            protected override void CheckStructure()
            {
                // TODO
            }
        }

        class MetaData : CompositeTag
        {

            private StringTag _clientId;

            private StringTag _machineId;

            private IntegerTag _sequenceNr;

            //Please do keep in mind that request time is in milliseconds!
            private IntegerTag _requestTime;

            public MetaData(TlvTag tag) : base(tag)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    switch (this[i].Type)
                    {
                        case 0x1:
                            _clientId = new StringTag(this[i]);
                            this[i] = _clientId;
                            break;
                        case 0x2:
                            _machineId = new StringTag(this[i]);
                            this[i] = _machineId;
                            break;
                        case 0x3:
                            _sequenceNr = new IntegerTag(this[i]);
                            this[i] = _sequenceNr;
                            break;
                        case 0x4:
                            _requestTime = new IntegerTag(this[i]);
                            this[i] = _requestTime;
                            break;
                    }
                }
            }

            protected override void CheckStructure()
            {
                // TODO
            }
        } 

        class ChainResult
        {
            private DataHash lastHash;
            private int level;

        }


        protected override void CheckStructure()
        {
            // TODO
        }
    }
}
