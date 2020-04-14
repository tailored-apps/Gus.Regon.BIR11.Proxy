using Gus.Regon.BIR11.WebService;
using Microsoft.Extensions.Options;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using WcfCoreMtomEncoder;

namespace Gus.Regon.BIR11.Proxy
{
    public class Client
    {
        private readonly IOptions<BirClientOptions> birClientOptions;
        private readonly UslugaBIRzewnPublClient uslugaBIRzewnPubl;
        public Client(IOptions<BirClientOptions> birClientOptions)
        {
            this.birClientOptions = birClientOptions;
            uslugaBIRzewnPubl = new UslugaBIRzewnPublClient(GetDefaultBinding(), GetDefaultEndpointAddress());
        }
        private void SetSid(string sid)
        {
            OperationContextScope scope = new OperationContextScope(uslugaBIRzewnPubl.InnerChannel);
            HttpRequestMessageProperty requestProperties = new HttpRequestMessageProperty();
            requestProperties.Headers.Add("sid", sid);
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperties;
        }
        public DanePobierzPelnyRaportResponse DanePobierzPelnyRaport(DanePobierzPelnyRaportRequest request)
        {
            return uslugaBIRzewnPubl.DanePobierzPelnyRaport(request);
        }

        public Task<DanePobierzPelnyRaportResponse> DanePobierzPelnyRaportAsync(DanePobierzPelnyRaportRequest request)
        {
            return uslugaBIRzewnPubl.DanePobierzPelnyRaportAsync(request);
        }

        public DanePobierzRaportZbiorczyResponse DanePobierzRaportZbiorczy(DanePobierzRaportZbiorczyRequest request)
        {
            return uslugaBIRzewnPubl.DanePobierzRaportZbiorczy(request);
        }

        public Task<DanePobierzRaportZbiorczyResponse> DanePobierzRaportZbiorczyAsync(DanePobierzRaportZbiorczyRequest request)
        {
            return uslugaBIRzewnPubl.DanePobierzRaportZbiorczyAsync(request);
        }

        public DaneSzukajPodmiotyResponse DaneSzukajPodmioty(DaneSzukajPodmiotyRequest request)
        {
            return uslugaBIRzewnPubl.DaneSzukajPodmioty(request);
        }

        public Task<DaneSzukajPodmiotyResponse> DaneSzukajPodmiotyAsync(DaneSzukajPodmiotyRequest request)
        {
            return uslugaBIRzewnPubl.DaneSzukajPodmiotyAsync(request);
        }

        public GetValueResponse GetValue(GetValueRequest request)
        {
            return uslugaBIRzewnPubl.GetValue(request);
        }

        public Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            return uslugaBIRzewnPubl.GetValueAsync(request);
        }

        public WylogujResponse Wyloguj(WylogujRequest request)
        {
            return uslugaBIRzewnPubl.Wyloguj(request);
        }

        public Task<WylogujResponse> WylogujAsync(WylogujRequest request)
        {
            return uslugaBIRzewnPubl.WylogujAsync(request);
        }

        public ZalogujResponse Zaloguj()
        {
            var response = uslugaBIRzewnPubl.Zaloguj(new ZalogujRequest { pKluczUzytkownika = birClientOptions.Value.UserKey });
            this.SetSid(response.ZalogujResult);
            return response;
        }

        public Task<ZalogujResponse> ZalogujAsync(ZalogujRequest request)
        {
            return uslugaBIRzewnPubl.ZalogujAsync(request);
        }

        private static Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.e3))
            {
                CustomBinding result = new CustomBinding();
                MtomMessageEncoderBindingElement textBindingElement = new MtomMessageEncoderBindingElement(new TextMessageEncodingBindingElement());

                result.Elements.Add(textBindingElement);
                HttpsTransportBindingElement httpsBindingElement = new HttpsTransportBindingElement();
                httpsBindingElement.AllowCookies = true;
                httpsBindingElement.MaxBufferSize = int.MaxValue;
                httpsBindingElement.MaxReceivedMessageSize = int.MaxValue;
                result.Elements.Add(httpsBindingElement);
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }

        private EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.e3))
            {
                return new EndpointAddress(birClientOptions.Value.EndpointAddress);
            }
            throw new InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }

        private Binding GetDefaultBinding()
        {
            return GetBindingForEndpoint(EndpointConfiguration.e3);
        }

        private EndpointAddress GetDefaultEndpointAddress()
        {
            return GetEndpointAddress(EndpointConfiguration.e3);
        }


        public enum EndpointConfiguration
        {
            e3,
        }
    }
}
