using PCEFTPOS.EFTClient.IPInterface;
using System;
using Xunit;

namespace IPInterface.TestPOS.Test
{
    public class MessageParserTest
    {
        [Fact]
        public void ParseCloudLogonResponse_EFTCloudLogonResponse()
        {
            IMessageParser msgParser = new DefaultMessageParser();
            EFTResponse response;
            EFTCloudLogonResponse eftCloudLogonResponse;

            // Successful EFTCloudLogonResponse
            response = msgParser.StringToEFTResponse("A0100CLOUD LOGON SUCCESS ");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudLogonResponse);
            eftCloudLogonResponse = response as EFTCloudLogonResponse;
            Assert.True(eftCloudLogonResponse.Success);
            Assert.True(eftCloudLogonResponse.ResponseCode == "00");
            Assert.True(eftCloudLogonResponse.ResponseText == "CLOUD LOGON SUCCESS ");

            // Failed EFTCloudPairResponse
            response = msgParser.StringToEFTResponse("A00XXCLOUD LOGON FAILURE ");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudLogonResponse);
            eftCloudLogonResponse = response as EFTCloudLogonResponse;
            Assert.False(eftCloudLogonResponse.Success);
            Assert.True(eftCloudLogonResponse.ResponseCode == "XX");
            Assert.True(eftCloudLogonResponse.ResponseText == "CLOUD LOGON FAILURE ");
        }

        [Fact]
        public void ParseCloudLogonResponse_EFTCloudPairResponse()
        {
            IMessageParser msgParser = new DefaultMessageParser();
            EFTResponse response;
            EFTCloudPairResponse eftCloudPairResponse;

            // Successful EFTCloudPairResponse
            response = msgParser.StringToEFTResponse("AP100CLOUD PAIR SUCCESS  005TOKEN");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudPairResponse);
            eftCloudPairResponse = response as EFTCloudPairResponse;
            Assert.True(eftCloudPairResponse.Success);
            Assert.True(eftCloudPairResponse.ResponseCode == "00");
            Assert.True(eftCloudPairResponse.ResponseText == "CLOUD PAIR SUCCESS  ");
            Assert.True(eftCloudPairResponse.Token == "TOKEN");

            // Successful v1 EFTCloudPairResponse
            response = msgParser.StringToEFTResponse("AP100CLOUD PAIR SUCCESS  000443013cloud.address005TOKEN");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudPairResponse);
            eftCloudPairResponse = response as EFTCloudPairResponse;
            Assert.True(eftCloudPairResponse.Success);
            Assert.True(eftCloudPairResponse.ResponseCode == "00");
            Assert.True(eftCloudPairResponse.ResponseText == "CLOUD PAIR SUCCESS  ");
            Assert.True(eftCloudPairResponse.Token == "TOKEN");

            // Failed EFTCloudPairResponse
            response = msgParser.StringToEFTResponse("AP0XXCLOUD PAIR FAILURE  ");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudPairResponse);
            eftCloudPairResponse = response as EFTCloudPairResponse;
            Assert.False(eftCloudPairResponse.Success);
            Assert.True(eftCloudPairResponse.ResponseCode == "XX");
            Assert.True(eftCloudPairResponse.ResponseText == "CLOUD PAIR FAILURE  ");
            Assert.True(eftCloudPairResponse.Token == "");
        }

        [Fact]
        public void ParseCloudLogonResponse_EFTCloudTokenLogonResponse()
        {
            IMessageParser msgParser = new DefaultMessageParser();
            EFTResponse response;
            EFTCloudTokenLogonResponse eftCloudTokenLogonResponse;

            // Successful EFTCloudTokenLogonResponse
            response = msgParser.StringToEFTResponse("AT100CLOUD PAIR SUCCESS  ");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudTokenLogonResponse);
            eftCloudTokenLogonResponse = response as EFTCloudTokenLogonResponse;
            Assert.True(eftCloudTokenLogonResponse.Success);
            Assert.True(eftCloudTokenLogonResponse.ResponseCode == "00");
            Assert.True(eftCloudTokenLogonResponse.ResponseText == "CLOUD PAIR SUCCESS  ");

            // Failed EFTCloudTokenLogonResponse
            response = msgParser.StringToEFTResponse("AT0XXCLOUD PAIR FAILURE  ");
            Assert.NotNull(response);
            Assert.True(response is EFTCloudTokenLogonResponse);
            eftCloudTokenLogonResponse = response as EFTCloudTokenLogonResponse;
            Assert.False(eftCloudTokenLogonResponse.Success);
            Assert.True(eftCloudTokenLogonResponse.ResponseCode == "XX");
            Assert.True(eftCloudTokenLogonResponse.ResponseText == "CLOUD PAIR FAILURE  ");
        }

    }
}
