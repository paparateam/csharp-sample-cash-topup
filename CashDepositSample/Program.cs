namespace CashDepositSample
{
    using System;
    using System.Net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using RestSharp;
    using RestSharp.Serializers;

    public class Program
    {
        private const string apiKey = "YOUR-API-KEY-HERE"; // Üye işyeri portalından alınacak API Key.

        private static void Main()
        {
            decimal amount = 99.99m;

            string receiversPhoneNumber = "+905327101289";

            MakeCashDepositRequest(amount, receiversPhoneNumber);
        }

        /// <summary>
        /// Fiziksel noktadan para yükleme isteği örneği
        /// </summary>
        private static void MakeCashDepositRequest(decimal amountToSend, string receiversPhoneNumber)
        {
            dynamic cashDeposit = new
            {
                amount = amountToSend,
                phoneNumber = receiversPhoneNumber
            };

            IRestResponse response = RequestHelper.PostJson("https://merchantapi-test-master.papara.com/cashdeposit", cashDeposit, apiKey);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine("Papara isteği karşılayamadı. Sistemsel bir hata oluştu");
                }

                if (response.StatusCode != HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Verilen API key yanlış ya da beklenmeyen bir IP'den istek yapıldı");
                }

                Console.WriteLine("İşlem başarısız oldu.");

                return;
            }

            var resultObj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (resultObj.succeeded == "false")
            {
                /*
                 * İşlem sonucu başarısız ise sonuç nesnesi içerisindeki error alanı hatanın mesajını ve hata kodunu içerecektir.
                 * Tablodaki hata kodu, metodun döndüğü JSON objesindeki error.code alanında denk gelir.
                 * 
                 * 100	Bilgileri verilen kullanıcı bulunamadı.
                 * 101	Üye işyeri bilgilerinize ulaşılamadı.
                 * 105	Üye işyeri hesabınızda yeterli bakiye yok.
                 * 107	Kullanıcı bu işlem ile birlikte bakiye limitini aşıyor.
                 * 111	Kullanıcı bu işlem ile birlikte aylık işlem limitini aşıyor.
                 * 112	En az para yatırma limitinin altına bir tutar gönderildi.
                 * 203	Kullanıcı hesabı bloke edilmiş durumda.
                 * 997	Nakit para yükleme işlemi yapma yetkisi hesabınıza tanımlanmamış durumda. Müşteri temsilciniz ile görüşmelisiniz.
                 * 998	Gönderdiğiniz parametreler beklenen formatta değil. Örnek: zorunlu alanlardan birisinin sağlanmamış olması.
                 * 999	Papara sisteminde bir hata oluştu.
                 * 
                 */

                Console.WriteLine("İşlem başarısız oldu. Alınan hatanın kodu: " + resultObj.error.code + " hata mesajı: " + resultObj.error.message);

                return;
            }

            Console.WriteLine("İşlem başarılı");
        }
    }

    /// <summary>
    /// Make it easy to make requests.
    /// </summary>
    public static class RequestHelper
    {
        /// <summary>
        /// Method to make it easy to post json to given endpoint that requires ApiKey auth.
        /// </summary>
        public static IRestResponse PostJson(
            string requestUrl,
            dynamic model,
            string apiKey)
        {
            var client = new RestClient(requestUrl);

            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CamelCaseSerializer()
            };

            request.AddHeader("ApiKey", apiKey);

            request.AddBody(model);

            IRestResponse response = client.Execute(request);

            return response;
        }
    }

    /// <summary>
    /// Enables the RequestHelper class to serialize objects to JSON using "camelCase" serialization.
    /// </summary>
    public class CamelCaseSerializer : ISerializer
    {
        /// <summary>
        /// Default serializer
        /// </summary>
        public CamelCaseSerializer()
        {
            ContentType = "application/json";
        }

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj)
        {
            var camelCaseSetting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string camelCaseJson = JsonConvert.SerializeObject(obj, camelCaseSetting);

            return camelCaseJson;
        }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }
}
