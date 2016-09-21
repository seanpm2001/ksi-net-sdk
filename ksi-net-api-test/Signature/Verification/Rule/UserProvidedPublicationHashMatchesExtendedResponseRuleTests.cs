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

using System.IO;
using Guardtime.KSI.Exceptions;
using Guardtime.KSI.Parser;
using Guardtime.KSI.Publication;
using Guardtime.KSI.Signature;
using Guardtime.KSI.Signature.Verification;
using Guardtime.KSI.Signature.Verification.Rule;
using Guardtime.KSI.Utils;
using NUnit.Framework;

namespace Guardtime.KSI.Test.Signature.Verification.Rule
{
    [TestFixture]
    public class UserProvidedPublicationHashMatchesExtendedResponseRuleTests
    {
        [Test]
        public void TestMissingContext()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Argument null exception when no context
            Assert.Throws<KsiException>(delegate
            {
                rule.Verify(null);
            });
        }

        [Test]
        public void TestMissingContextSignature()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Verification exception on missing KSI signature 
            Assert.Throws<KsiVerificationException>(delegate
            {
                TestVerificationContext context = new TestVerificationContext();

                rule.Verify(context);
            });
        }

        [Test]
        public void TestSignatureWithoutContextUserPublication()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Check signature without user publication
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignatureDo_Ok), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream, false)
                };

                Assert.Throws<KsiVerificationException>(delegate
                {
                    rule.Verify(context);
                });
            }
        }

        [Test]
        public void TestSignatureWithInvalidContextExtendFunctions()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Check invalid extended calendar chain from context extension function
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignatureDo_Ok), FileMode.Open))
            {
                TestVerificationContextFaultyFunctions context = new TestVerificationContextFaultyFunctions()
                {
                    Signature = new KsiSignatureFactory().Create(stream, false),
                    UserPublication = new PublicationData("AAAAAA-CVZ2AQ-AAIVXJ-PLJDAG-JMMYUC-OTP2GA-ELBIDQ-OKDY3C-C3VEH2-AR35I2-OJUACP-GOGD6K")
                };

                Assert.Throws<KsiVerificationException>(delegate
                {
                    rule.Verify(context);
                });
            }
        }

        [Test]
        public void TestRfc3161SignatureExtendedCalendarHashWithContextUserPublication()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Check legacy signature with publication record
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignatureDo_Legacy_Ok_With_Publication_Record), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream, false),
                    ExtendedCalendarHashChain =
                        new CalendarHashChain(new RawTag(Constants.CalendarHashChain.TagType, false, false,
                            Base16.Decode(
                                "010455CE81000204538F88D3052101145C6CDA9F901A65C0B3C09896675928E1A85977280BF65C0A638C8A47BB358A082101D59926FE0A1BB9DA1C543BB83595327083D1B31BC924CF646126B84A1917B68908210193A28F9135B77A59A9272B52D770BCEC86A289DAB9AB04675DC686FDBAA1E2650721011576EBF50B7F57F291C630C4447F6C2D7C53FFB01B1A8E6D03C255B6D8192E1A072101351F142994A684EDEF7F36FC97D670CD44DAF657F0A364695E99AFE82A12185D0821011FA0610ABC70C14A57FB8AE7F7B3ACEF06158EFD7FEA72B8D283B6250816B9B80721011952CC826A54D06305221695780BA6D2A28A40E219A5E4E0286B6BF4786978F70821010B55DA6AC77E5E4D4DF01B3F7B7A4A54C40649EF8B7A6FFD374A35B0AFE6B172082101772BC22F2AE1EB91CE7004E08958110748DFD2DEBD0BDD52F1D4071728CC61F80721015F23E60BA799C079C32F70409EBADD170FFDCEE6EF9D07C0A8A42CE5AC7DE847072101C25AED220B4072B5BEC1E7CAF84D7B4E7336FE2F78B4BAB62989A20E60C3D126072101DDB4DB0658527FB6897B3C3142512B0637F0359B7FE164C0CEB7B5F16B655905082101449D3D06E1A50565F5C0D9523465804D268C74384B22E08D9B9AE239A23147F5072101E0276D72C61E4D161C90BE3AEEC3CF3E51870FFA8D373790D5561E901389551E0721013FCE89BD33B202C9F775E3CE59A272D8AD2B17F7A8C944C76F87FA040F9D16D70721015643670BDD1B6DEBBA05242FEB7423D7F29016401BE4C0F2E1AA9E824919E236082101DE0A153233AC4AB779C2DD9FB798937EA95728E9ABF12103BDC60BE2DB0DADE6082101A4A4B2F924477698DD230F25F8D8FA9F0FE9AB2AE6964F799E536C3FC396B5C4082101F965C19A6248ACFC6ADEF395208D352C1FBE3548484C747045FAE6FA7B98A471082101C6585B401E35921807FCD7E7312E5F317DD67A615D8ACA516F3DC47CD584D1520821014BEB537B59DA957DF787F0B48B313AB1565FF005B23F23A3C6B17EAC43A4EC2F0721014F91EC53886806917F95B6B901A7932A1ACB330E65068669B86908C302509DDF07210171247D359734FCF9DC706B1B1116D79266DCD4CAC6A84C32F575B34F366F3EA10721010F12570DDD1C833633CBBA07E93100DD83D7C85E1CE8BC33801D8913C0566BB60821016B303486CE63811ECCF8FB5EE071E471C574E661FBCAE366F8F4DC6ACFC79C400821011C102667AC4FBC8D91B99EF4A7C78BEE2448FF52AA6CD1D557595F23510E98EA082101FB79B43E0AA6BEE9173839C051D3D0DAC6F8EFBD487331B5B86A214C42FAA81C0721014E906ED3502247AAE4D142C8766176D7A403D413A7E0CD49F1872179FC5D6CDA082101496FC0120D854E7534B992AB32EC3045B20D4BEE1BFBE4564FD092CEAFA08B72082101BB44FD36A5F3CDEE7B5C6DF3A6098A09E353335B6029F1477502588A7E37BE00"))),
                    UserPublication = new PublicationData("AAAAAA-CVZ2AQ-AAIVXJ-PLJDAG-JMMYUC-OTP2GA-ELBIDQ-OKDY3C-C3VEH2-AR35I2-OJUACP-GOGD6K")
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Ok, verificationResult.ResultCode);
            }
        }

        [Test]
        public void TestSignatureExtendedCalendarHashWithContextUserPublication()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            // Check signature with publication record
            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignatureDo_Ok_With_Publication_Record), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream, false),
                    ExtendedCalendarHashChain =
                        new CalendarHashChain(new RawTag(Constants.CalendarHashChain.TagType, false, false,
                            Base16.Decode(
                                "010455CE8100020453B2A01D052101E8E1FB57586DFE67D5B541FCE6CFC78684E4C38ED87784D97FFA7FEB81DB7D1E08210153D8E4B360DDB59CE2C6028C5C502664C035165C8FB4462BF82340F550E7547C07210109867731331680C0A0605E59522C35060DD6BA1F741EB411FECE2D446F6E66C60821015744E0D040C744B1DB86070EB49051DE95F983E7288ED9F489C508B8B6DA6DF2082101736BA09B6108819611B3F6400415E7F778E21EAA412D1B769A048A657B9E98380821019D07C73BECEC4F37F05132C8695DED4D2EA27432DAB5A75E19739D42C667BD7B072101982F75FE7D976EF4F09334F9C74DAF0F8D9E6F21BC15E1E5B0459C5FB2CCDA94072101177769276EAED1F6E8975F53795CABC4E7AF15FA6E2EB82E953E75903125C5A70721011B3EF77E8A12037061A8D66C02C08017BE5989A5549C37AD29BE6DA1C91D22D3072101B3C58D951F365C7B5A3EA32D10C93681BE2587F43516172A9207F80BCF091C280721011A874BCCEC382D3E262FCBD45E98AC015D33EBA11A4BBA235EF4DD1CD74FB74D072101B79DEFB98B7399FE191777D9D5E7BD65EBFEB437A67E03410AA447129B0F904F072101D8309245A4A52C0A85C72855B217F941215B2FA4F9867D5BA7EE717609614BC5072101CC2DA8F01BC8E09E745717F574484D02772D1CAA711E11ACAEB336A3FBBA974708210181F5BA98D16F8A14B2B7E7C2D5FEE8FCD30D61731D097571ADF18BAF0285807C072101832753B33D49A83863338CE03FD7D11177344978754988963A6C3C8CDB31642F0821013110D9D04908F59E422A9C674721ABF6E78B5BFB9B62F98D38E35818D28110CA072101CE9DDC7F1BF91CAFFFA71867F9F988826DAA8C6157952228758FE5F21C6D106F082101C1DFAFE010C4D7046E143BBA14781F206D0026710C7B6D633A446B86750C54CE072101B5305FC528B235A33B32A4DD64575FED5802F7658F5FF8A4C0AA237EB50A98AE072101CEF6F8C65E2DD16E4ED42DA884B59886BB781D4047BC39FFD0771D5A5617E73F08210105D3A7B71DA6715B9B4F7DA5E4ECF52584B1A1A3451B814E4633C8894EC656CA08210142FEAC12960B2147E41C2C89663C2D05D11AECB43A5E646BB241F53C8BF00C010721010F12570DDD1C833633CBBA07E93100DD83D7C85E1CE8BC33801D8913C0566BB60821016B303486CE63811ECCF8FB5EE071E471C574E661FBCAE366F8F4DC6ACFC79C400821011C102667AC4FBC8D91B99EF4A7C78BEE2448FF52AA6CD1D557595F23510E98EA082101FB79B43E0AA6BEE9173839C051D3D0DAC6F8EFBD487331B5B86A214C42FAA81C0721014E906ED3502247AAE4D142C8766176D7A403D413A7E0CD49F1872179FC5D6CDA082101496FC0120D854E7534B992AB32EC3045B20D4BEE1BFBE4564FD092CEAFA08B72082101BB44FD36A5F3CDEE7B5C6DF3A6098A09E353335B6029F1477502588A7E37BE00"))),
                    UserPublication = new PublicationData("AAAAAA-CVZ2AQ-AAIVXJ-PLJDAG-JMMYUC-OTP2GA-ELBIDQ-OKDY3C-C3VEH2-AR35I2-OJUACP-GOGD6K")
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Ok, verificationResult.ResultCode);
            }
        }

        [Test]
        public void TestSignatureInvalidExtendedCalendarHashWithUserPublication()
        {
            UserProvidedPublicationHashMatchesExtendedResponseRule rule = new UserProvidedPublicationHashMatchesExtendedResponseRule();

            using (FileStream stream = new FileStream(Path.Combine(TestSetup.LocalPath, Properties.Resources.KsiSignatureDo_Ok), FileMode.Open))
            {
                TestVerificationContext context = new TestVerificationContext()
                {
                    Signature = new KsiSignatureFactory().Create(stream, false),
                    ExtendedCalendarHashChain =
                        new CalendarHashChain(new RawTag(Constants.CalendarHashChain.TagType, false, false,
                            Base16.Decode(
                                "0104538FB3000204538F88D3052101145C6CDA9F901A65C0B3C09896675928E1A85977280BF65C0A638C8A47BB358A082101D59926FE0A1BB9DA1C543BB83595327083D1B31BC924CF646126B84A1917B68908210193A28F9135B77A59A9272B52D770BCEC86A289DAB9AB04675DC686FDBAA1E265072101000000000000000000000000000000000000000000000000000000000000000007210100000000000000000000000000000000000000000000000000000000000000000821011FA0610ABC70C14A57FB8AE7F7B3ACEF06158EFD7FEA72B8D283B6250816B9B807210100000000000000000000000000000000000000000000000000000000000000000821010B55DA6AC77E5E4D4DF01B3F7B7A4A54C40649EF8B7A6FFD374A35B0AFE6B172082101772BC22F2AE1EB91CE7004E08958110748DFD2DEBD0BDD52F1D4071728CC61F8072101000000000000000000000000000000000000000000000000000000000000000007210100000000000000000000000000000000000000000000000000000000000000000721010000000000000000000000000000000000000000000000000000000000000000082101449D3D06E1A50565F5C0D9523465804D268C74384B22E08D9B9AE239A23147F507210100000000000000000000000000000000000000000000000000000000000000000721010000000000000000000000000000000000000000000000000000000000000000082101DE0A153233AC4AB779C2DD9FB798937EA95728E9ABF12103BDC60BE2DB0DADE6082101A4A4B2F924477698DD230F25F8D8FA9F0FE9AB2AE6964F799E536C3FC396B5C4082101F965C19A6248ACFC6ADEF395208D352C1FBE3548484C747045FAE6FA7B98A471082101C6585B401E35921807FCD7E7312E5F317DD67A615D8ACA516F3DC47CD584D1520821014BEB537B59DA957DF787F0B48B313AB1565FF005B23F23A3C6B17EAC43A4EC2F0821016B303486CE63811ECCF8FB5EE071E471C574E661FBCAE366F8F4DC6ACFC79C400821011C102667AC4FBC8D91B99EF4A7C78BEE2448FF52AA6CD1D557595F23510E98EA082101FB79B43E0AA6BEE9173839C051D3D0DAC6F8EFBD487331B5B86A214C42FAA81C082101496FC0120D854E7534B992AB32EC3045B20D4BEE1BFBE4564FD092CEAFA08B72082101BB44FD36A5F3CDEE7B5C6DF3A6098A09E353335B6029F1477502588A7E37BE00"))),
                    UserPublication = new PublicationData("AAAAAA-CVZ2AQ-AAIVXJ-PLJDAG-JMMYUC-OTP2GA-ELBIDQ-OKDY3C-C3VEH2-AR35I2-OJUACP-GOGD6K")
                };

                VerificationResult verificationResult = rule.Verify(context);
                Assert.AreEqual(VerificationResultCode.Fail, verificationResult.ResultCode);
                Assert.AreEqual(VerificationError.Pub01, verificationResult.VerificationError);
            }
        }
    }
}