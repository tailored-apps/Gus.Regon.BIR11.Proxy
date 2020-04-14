using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Serialization;
using Xunit;

namespace Gus.Regon.BIR11.Proxy.Itegration.Tests
{
    public class BusinessEntityFind
    {
        [Fact]
        public void CanLogInAndGetBusinessEntity()
        {
            var clientOptions = new BirClientOptions() { EndpointAddress = "https://wyszukiwarkaregontest.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc", UserKey = "abcde12345abcde12345" };
            Moq.Mock<IOptions<BirClientOptions>> birClientOptionsMock = new Moq.Mock<IOptions<BirClientOptions>>();
            birClientOptionsMock.Setup(x => x.Value).Returns(clientOptions);

            var client = new Client(birClientOptionsMock.Object);
            var zalogujResponse = client.Zaloguj();
            var valueResponse = client.GetValue(new WebService.GetValueRequest { Body = new WebService.GetValueRequestBody { pNazwaParametru = "KomunikatKod"} });

            var parametryWyszukiwania = new WebService.ParametryWyszukiwania();
            parametryWyszukiwania.Nip = "7710108472";

            try
            {
                var daneSzukajPodmiotyResponse = client.DaneSzukajPodmioty(new WebService.DaneSzukajPodmiotyRequest { pParametryWyszukiwania = parametryWyszukiwania });
                Assert.NotEmpty(daneSzukajPodmiotyResponse.DaneSzukajPodmiotyResult);

                using (var reader = new StringReader(daneSzukajPodmiotyResponse.DaneSzukajPodmiotyResult))
                {
                    XmlSerializer daneSzukajSerializer = new XmlSerializer(typeof(Models.DaneSzukajPodmioty.DaneSzukajPodmioty.root));
                    var daneSzukaj = (Models.DaneSzukajPodmioty.DaneSzukajPodmioty.root)daneSzukajSerializer.Deserialize(reader);

                    Assert.NotEmpty(daneSzukaj.dane);
                    Assert.True(daneSzukaj.dane.Length>0);
                }
            }
            catch (Exception ex)
            {
                var value2 = client.GetValue(new WebService.GetValueRequest { Body = new WebService.GetValueRequestBody { pNazwaParametru = "KomunikatKod" } });
                throw new Exception(value2?.Body?.GetValueResult, ex);
            }
            finally
            {
                var wylogujResponse = client.Wyloguj(new WebService.WylogujRequest { pIdentyfikatorSesji = zalogujResponse.ZalogujResult });
            }
        }
    }
}
