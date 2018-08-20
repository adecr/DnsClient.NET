﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DnsClient.Protocol;
using Xunit;

namespace DnsClient.Tests
{
    [ExcludeFromCodeCoverage]
    public class DnsResponseParsingTest
    {
        private static readonly DnsRequestMessage _nullRequestMessage =
            new DnsRequestMessage(new DnsRequestHeader(0, DnsOpCode.Query), new DnsQuestion("bla", QueryType.A, QueryClass.IN));

        private readonly LookupClient _client;

        public DnsResponseParsingTest()
        {
            _client = new LookupClient(IPAddress.Loopback);
        }

        [Fact]
        public void DnsRecordFactory_McnetValidateSupport()
        {
            var types = (ResourceRecordType[])Enum.GetValues(typeof(ResourceRecordType));
            var result = _client.ResolveQuery(_client.Settings.ShuffleNameServers(), _client.Settings, new TestMessageHandler(), _nullRequestMessage);

            var ignore = new ResourceRecordType[]
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ResourceRecordType.MD,
                ResourceRecordType.MF,
#pragma warning restore CS0618 // Type or member is obsolete
                ResourceRecordType.OPT,
                ResourceRecordType.RRSIG
            };

            foreach (var t in types)
            {
                if (ignore.Contains(t))
                {
                    continue;
                }

                var numRecords = result.AllRecords.OfRecordType(t).Count();
                Assert.True(numRecords > 0, $"{t} should have records");
            }
        }

        [Fact]
        public void DnsRecordFactory_McnetA()
        {
            var result = _client.ResolveQuery(_client.Settings.ShuffleNameServers(), _client.Settings, new TestMessageHandler(), _nullRequestMessage);

            var records = result.Answers.ARecords().ToArray();

            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.1")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.2")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.20")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.21")));

            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.50")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.24")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.25")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.26")));
            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.27")));

            Assert.Contains(records, p => p.Address.Equals(IPAddress.Parse("192.168.178.30")));
        }

        [Fact]
        public void DnsRecordFactory_McnetAAAA()
        {
            var records = GetTypedRecords<AaaaRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Address.Equals(IPAddress.Parse("fe80::24c9:8e14:9df4:a314")));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetSSHFP()
        {
            var records = GetTypedRecords<SshfpRecord>();

            var validateRecord = records.FirstOrDefault();

            Assert.NotNull(validateRecord);
            Assert.Equal(SshfpAlgorithm.RSA, validateRecord.Algorithm);
            Assert.Equal(SshfpFingerprintType.SHA1, validateRecord.FingerprintType);
            Assert.Equal("9DBA55CEA3B8E15528665A6781CA7C35190CF0EC", validateRecord.Fingerprint);
        }

        [Fact]
        public void DnsRecordFactory_McnetAfsDb()
        {
            var records = GetTypedRecords<AfsDbRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Hostname.Equals("srv2.mcnet.com."));

            Assert.NotNull(validateRecord);
            Assert.Equal(AfsType.Afs, validateRecord.SubType);
        }

        [Fact]
        public void DnsRecordFactory_McnetCaa()
        {
            var records = GetTypedRecords<CaaRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Tag.Equals("policy"));

            Assert.NotNull(validateRecord);
            Assert.Equal(1, validateRecord.Flags);
            Assert.Equal("1.3.6.1.4.1.35405.666.1", validateRecord.Value);
        }

        [Fact]
        public void DnsRecordFactory_McnetCName()
        {
            var records = GetTypedRecords<CNameRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.CanonicalName.Equals("mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetHInfo()
        {
            var records = GetTypedRecords<HInfoRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Cpu.Equals("Intel-I7"));

            Assert.NotNull(validateRecord);
            Assert.Equal("WINDOWS", validateRecord.OS);
        }

        [Fact]
        public void DnsRecordFactory_McnetMB()
        {
            var records = GetTypedRecords<MbRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.MadName.Equals("srv.mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetMG()
        {
            var records = GetTypedRecords<MgRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.MgName.Equals("hidden.mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetMInfo()
        {
            var records = GetTypedRecords<MInfoRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.RMailBox.Equals("hidden.mcnet.com."));

            Assert.NotNull(validateRecord);
            Assert.Equal("hidden2.mcnet.com.", validateRecord.EmailBox);
        }

        [Fact]
        public void DnsRecordFactory_McnetMR()
        {
            var records = GetTypedRecords<MrRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.NewName.Equals("xn--4gbrim.xn----ymcbaaajlc6dj7bxne2c.xn--wgbh1c.mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetMX()
        {
            var records = GetTypedRecords<MxRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Exchange.Equals("mail.mcnet.com."));

            Assert.NotNull(validateRecord);
            Assert.Equal(10, validateRecord.Preference);
        }

        [Fact]
        public void DnsRecordFactory_McnetNS()
        {
            var records = GetTypedRecords<NsRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.NSDName.Equals("ns1.mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetPTR()
        {
            var records = GetTypedRecords<PtrRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.PtrDomainName.Equals("hidden.mcnet.com."));

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetRP()
        {
            var records = GetTypedRecords<RpRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.MailboxDomainName.Equals("mia.c.mcnet.com."));

            Assert.NotNull(validateRecord);
            Assert.Equal("addr.mia.mcnet.com.", validateRecord.TextDomainName);
        }

        [Fact]
        public void DnsRecordFactory_McnetSOA()
        {
            var records = GetTypedRecords<SoaRecord>();

            var validateRecord = records.FirstOrDefault();

            Assert.NotNull(validateRecord);
            Assert.Equal(2419200, (int)validateRecord.Expire);
            Assert.Equal(604800, (int)validateRecord.Minimum);
            Assert.Equal("ns1.mcnet.com.", validateRecord.MName);
            Assert.Equal(604800, (int)validateRecord.Refresh);
            Assert.Equal(86400, (int)validateRecord.Retry);
            Assert.Equal("hostmaster.mcnet.com.", validateRecord.RName);
            Assert.Equal(2017010600, (int)validateRecord.Serial);
            Assert.Equal(100, (int)validateRecord.InitialTimeToLive);
        }

        [Fact]
        public void DnsRecordFactory_McnetSRV()
        {
            var records = GetTypedRecords<SrvRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Target.Equals("srv3.mcnet.com."));

            Assert.NotNull(validateRecord);
            Assert.Equal(0, validateRecord.Priority);
            Assert.Equal(0, validateRecord.Weight);
            Assert.Equal(7002, validateRecord.Port);
        }

        [Fact]
        public void DnsRecordFactory_McnetTXT()
        {
            var records = GetTypedRecords<TxtRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.EscapedText.Contains("(text with)"));

            // this one has some special text with escaped text
            Assert.Equal(7, validateRecord.Text.Count);
            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetURI()
        {
            var records = GetTypedRecords<UriRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Target.Equals("ftp://srv.mcnet.com/public", StringComparison.OrdinalIgnoreCase)
                && p.Priority == 10
                && p.Weigth == 1);

            Assert.NotNull(validateRecord);
        }

        [Fact]
        public void DnsRecordFactory_McnetWKS()
        {
            var records = GetTypedRecords<WksRecord>();

            var validateRecord = records.FirstOrDefault(
                p => p.Protocol == System.Net.Sockets.ProtocolType.Tcp
                && p.Address.Equals(IPAddress.Parse("192.168.178.25"))
                && p.Ports.Contains(9));

            Assert.NotNull(validateRecord);
        }

        private TRecord[] GetTypedRecords<TRecord>()
            where TRecord : DnsResourceRecord
        {
            var result = _client.ResolveQuery(_client.Settings.ShuffleNameServers(), _client.Settings, new TestMessageHandler(), _nullRequestMessage);

            var records = result.Answers.OfType<TRecord>().ToArray();
            return records;
        }

        private class TestMessageHandler : DnsMessageHandler
        {
            private const int MaxSize = 4096;

            // raw bytes from mcnet.com
            private static readonly byte[] ZoneData = new byte[]
            {
                92,159,132,128,0,1,0,65,0,0,0,1,5,109,99,110,101,116,3,99,111,109,0,0,252,0,1,192,12,0,6,0,1,0,0,0,100,0,39,3,110,115,49,192,12,10,104,111,115,116,109,97,115,116,101,114,192,12,120,57,35,168,0,9,58,128,0,1,81,128,0,36,234,0,0,9,58,128,192,12,0,16,0,1,0,0,0,100,0,68,4,109,111,114,101,11,40,116,101,120,116,32,119,105,116,104,41,17,34,115,112,101,99,105,97,108,34,32,195,182,195,164,195,156,33,6,194,167,36,37,92,47,6,64,115,116,117,102,102,4,59,97,110,100,13,59,102,97,107,101,32,99,111,109,109,101,110,116,192,12,0,16,0,1,0,0,0,100,0,53,4,115,111,109,101,4,116,101,120,116,12,115,101,112,97,114,97,116,101,100,32,98,121,5,115,112,97,99,101,4,119,105,116,104,3,110,101,119,4,108,105,110,101,5,115,116,117,102,102,3,116,111,111,192,12,0,16,0,1,0,0,0,100,0,76,7,97,110,111,116,104,101,114,4,109,111,114,101,11,40,116,101,120,116,32,119,105,116,104,41,17,34,115,112,101,99,105,97,108,34,32,195,182,195,164,195,156,33,6,194,167,36,37,92,47,6,64,115,116,117,102,102,4,59,97,110,100,13,59,102,97,107,101,32,99,111,109,109,101,110,116,192,12,0,16,0,1,0,0,0,100,0,49,48,115,111,109,101,32,108,111,110,103,32,116,101,120,116,32,119,105,116,104,32,115,111,109,101,32,115,116,117,102,102,32,105,110,32,105,116,32,123,108,97,108,97,58,98,108,117,98,125,192,12,0,7,0,1,0,0,0,100,0,6,3,115,114,118,192,12,192,12,0,7,0,1,0,0,0,100,0,62,15,108,195,156,195,164,39,108,97,195,188,195,182,35,50,120,22,88,78,45,45,67,76,67,72,67,48,69,65,48,66,50,71,50,65,57,71,67,68,11,88,78,45,45,48,90,87,77,53,54,68,5,109,99,110,101,116,3,99,111,109,0,192,12,0,8,0,1,0,0,0,100,0,9,6,104,105,100,100,101,110,192,12,192,12,0,9,0,1,0,0,0,100,0,10,7,104,105,100,100,101,110,50,192,12,192,12,0,9,0,1,0,0,0,100,0,60,10,120,110,45,45,52,103,98,114,105,109,26,120,110,45,45,45,45,121,109,99,98,97,97,97,106,108,99,54,100,106,55,98,120,110,101,50,99,10,120,110,45,45,119,103,98,104,49,99,5,109,99,110,101,116,3,99,111,109,0,192,12,0,33,0,1,0,0,0,100,0,22,0,1,0,1,31,144,4,115,114,118,52,5,109,99,110,101,116,3,99,111,109,0,194,90,0,12,0,1,0,0,1,244,0,2,194,90,194,90,0,12,0,1,0,0,1,244,0,2,193,220,194,90,0,14,0,1,0,0,0,100,0,4,193,220,193,241,194,90,1,1,0,1,0,0,1,244,0,31,1,6,112,111,108,105,99,121,49,46,51,46,54,46,49,46,52,46,49,46,51,53,52,48,53,46,54,54,54,46,49,194,90,1,1,0,1,0,0,1,244,0,77,129,3,116,98,115,77,68,73,71,65,49,85,69,74,81,89,74,89,73,90,73,65,87,85,68,66,65,73,66,66,67,65,88,122,74,103,80,97,111,84,55,70,101,88,97,80,122,75,118,54,109,73,50,68,48,121,105,108,105,102,43,55,87,104,122,109,104,77,71,76,101,47,111,66,65,61,61,194,90,1,1,0,1,0,0,1,244,0,54,255,16,115,111,109,101,116,104,105,110,103,115,116,114,97,110,103,101,84,104,101,118,97,108,117,101,83,116,105,110,103,119,105,116,104,195,156,98,101,114,83,112,101,99,105,195,182,108,118,97,108,117,101,46,194,90,1,0,0,1,0,0,1,94,0,30,0,10,0,1,102,116,112,58,47,47,115,114,118,46,109,99,110,101,116,46,99,111,109,47,112,117,98,108,105,99,194,90,0,10,0,1,0,0,1,94,0,117,101,109,115,48,49,46,121,111,117,114,45,102,114,101,101,100,111,109,46,100,101,59,85,83,59,49,57,56,46,50,53,53,46,51,48,46,50,52,50,59,48,59,53,55,51,48,59,100,101,102,97,117,108,116,44,118,111,108,117,109,101,44,110,111,114,116,104,97,109,101,114,105,99,97,44,105,110,116,101,114,97,99,116,105,118,101,44,118,111,105,112,44,111,112,101,110,118,112,110,44,112,112,116,112,44,115,111,99,107,115,53,44,102,114,101,101,59,194,90,0,44,0,1,0,0,0,200,0,22,1,1,157,186,85,206,163,184,225,85,40,102,90,103,129,202,124,53,25,12,240,236,194,90,0,17,0,1,0,0,0,100,0,37,3,109,105,97,1,99,5,109,99,110,101,116,3,99,111,109,0,4,97,100,100,114,3,109,105,97,5,109,99,110,101,116,3,99,111,109,0,194,90,0,17,0,1,0,0,0,100,0,39,4,97,108,101,120,1,98,5,109,99,110,101,116,3,99,111,109,0,4,97,100,100,114,4,97,108,101,120,5,109,99,110,101,116,3,99,111,109,0,194,90,0,17,0,1,0,0,0,100,0,19,4,108,117,99,121,1,99,5,109,99,110,101,116,3,99,111,109,0,0,194,90,0,17,0,1,0,0,0,100,0,36,5,109,105,99,104,97,1,99,5,109,99,110,101,116,3,99,111,109,0,5,109,105,99,104,97,5,109,99,110,101,116,3,99,111,109,0,196,204,0,18,0,1,0,0,0,100,0,18,0,1,4,115,114,118,50,5,109,99,110,101,116,3,99,111,109,0,196,234,0,18,0,1,0,0,0,100,0,18,0,2,4,115,114,118,51,5,109,99,110,101,116,3,99,111,109,0,197,8,0,11,0,1,0,0,0,100,0,72,192,168,178,25,6,0,84,64,64,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,32,197,8,0,11,0,1,0,0,0,100,15,216,192,168,178,26,17,1,0,0,0,0,0,4,0,0,0,128,0,0,4,64,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,128,197,8,0,13,0,1,0,0,0,100,0,17,8,73,110,116,101,108,45,73,55,7,87,73,78,68,79,87,83,197,8,0,13,0,1,0,0,0,100,0,14,8,83,112,97,114,99,45,49,48,4,85,78,73,88,197,8,0,2,0,1,0,9,58,128,0,2,192,39,197,8,0,2,0,1,0,9,58,128,0,6,3,110,115,50,197,8,197,8,0,1,0,1,0,1,81,128,0,4,192,168,178,50,197,8,0,28,0,1,0,0,0,100,0,16,254,128,0,0,0,0,0,0,36,201,142,20,157,244,163,20,197,8,0,15,0,1,0,0,3,232,0,9,0,10,4,109,97,105,108,197,8,197,8,0,15,0,1,0,0,3,232,0,9,0,10,4,109,97,105,108,196,198,8,95,97,102,115,51,45,112,114,4,95,116,99,112,5,109,99,110,101,116,3,99,111,109,0,0,33,0,1,0,0,0,100,0,22,0,0,0,0,27,90,4,115,114,118,52,5,109,99,110,101,116,3,99,111,109,0,8,95,97,102,115,51,45,118,108,214,1,0,33,0,1,0,0,0,100,0,22,0,0,0,0,27,91,4,115,114,118,50,5,109,99,110,101,116,3,99,111,109,0,8,95,97,102,115,51,45,112,114,4,95,117,100,112,5,109,99,110,101,116,3,99,111,109,0,0,33,0,1,0,0,0,100,0,22,0,0,0,0,27,90,4,115,114,118,51,5,109,99,110,101,116,3,99,111,109,0,196,120,0,1,0,1,0,0,0,100,0,4,192,168,178,61,196,115,0,16,0,1,0,0,0,100,0,19,18,69,114,110,115,116,32,40,49,50,51,41,32,52,53,54,55,56,57,4,109,97,105,108,196,120,0,15,0,1,0,0,0,100,0,4,0,10,213,220,193,220,0,1,0,1,0,0,0,100,0,4,192,168,178,24,193,241,0,1,0,1,0,0,0,100,0,4,192,168,178,24,213,220,0,1,0,1,0,1,81,128,0,4,192,168,178,3,196,70,0,1,0,1,0,0,0,100,0,4,192,168,178,62,196,65,0,16,0,1,0,0,0,100,0,17,16,89,101,121,32,40,49,50,51,41,32,52,53,54,55,56,57,4,109,97,105,108,196,70,0,15,0,1,0,0,0,100,0,4,0,10,213,220,196,198,0,5,0,1,0,0,0,100,0,2,214,138,4,97,100,100,114,196,198,0,16,0,1,0,0,0,100,0,18,17,66,111,115,115,32,40,49,50,51,41,32,52,53,54,55,56,57,213,241,0,1,0,1,0,0,0,100,0,4,192,168,178,4,3,115,117,98,196,198,0,5,0,1,0,0,0,200,0,4,1,50,196,198,3,119,119,119,196,198,0,5,0,1,0,0,0,100,0,2,214,138,192,39,0,1,0,1,0,1,81,128,0,4,192,168,178,1,213,156,0,1,0,1,0,1,81,128,0,4,192,168,178,2,5,112,104,111,110,101,214,138,0,1,0,1,0,0,0,100,0,4,192,168,178,20,193,128,0,1,0,1,0,0,0,100,0,4,192,168,178,21,193,128,0,13,0,1,0,0,0,100,0,17,8,73,110,116,101,108,45,73,53,7,87,73,78,68,79,87,83,214,76,0,1,0,1,0,0,0,100,0,4,192,168,178,25,214,133,0,1,0,1,0,0,0,100,0,4,192,168,178,26,214,33,0,1,0,1,0,0,0,100,0,4,192,168,178,27,4,117,98,101,114,214,138,0,1,0,1,0,0,0,100,0,4,192,168,178,30,3,119,119,119,214,138,0,5,0,1,0,0,1,244,0,2,214,138,193,162,0,1,0,1,0,0,0,100,0,4,192,168,178,100,214,138,0,6,0,1,0,0,0,100,0,24,192,39,192,45,120,57,35,168,0,9,58,128,0,1,81,128,0,36,234,0,0,9,58,128,0,0,41,16,0,0,0,0,0,0,0
            };

            public TestMessageHandler()
            {
            }

            public override bool IsTransientException<T>(T exception)
            {
                return false;
            }

            public override DnsResponseMessage Query(
                IPEndPoint server,
                DnsRequestMessage request,
                TimeSpan timeout)
            {
                var response = GetResponseMessage(new ArraySegment<byte>(ZoneData, 0, ZoneData.Length));

                return response;
            }

            public override Task<DnsResponseMessage> QueryAsync(
                IPEndPoint server,
                DnsRequestMessage request,
                CancellationToken cancellationToken,
                Action<Action> cancelationCallback)
            {
                // no need to run async here as we don't do any IO
                return Task.FromResult(Query(server, request, Timeout.InfiniteTimeSpan));
            }
        }
    }
}