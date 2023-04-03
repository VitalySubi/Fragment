using System;
using System.Collections.Generic;

using System.Web.Mvc;
using System.Web.Routing;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;

namespace Zcab.Models
{
    // Запрос адреса объекта по кадастровому номеру с Росреестра
    public class RosreestrLinker
    {
        public string cad_num { get; set; }
        public int req_type { get; set; }
        public string address = "";

        public RosreestrLinker()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Отправляет http-запрос одним или двумя вариантами
        /// </summary>
        /// <returns></returns>
        public async Task SendRequest()
        {
            bool result = false;

            /// Стандартный запрос по отдельному API
            result = await SendRequestAPI();
            if (result) return;

            /// Иногда стандартный запрос по API не возвращает ожидаемый результат.
            /// Предположительно, это зависит от адреса, т.е. работает не совсеми адресами объектов,
            /// поэтому используется альтернативный запрос. Делается 5 попыток, поскольку иногда
            /// не возвращается результат с первого раза
            for (int i = 0; i < 5; i++)
            {
                result = await SendRequestTyped();
                if (result) return;
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Стандартный http-запрос по отдельному API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendRequestAPI()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var baseAddress = $"https://rosreestr.ru/api/online/fir_objects/{cad_num}";

                    client.BaseAddress = new Uri(baseAddress);

                    var response = await client.GetAsync(baseAddress);

                    if (response.IsSuccessStatusCode)
                    {
                        var stringData = await response.Content.ReadAsStringAsync();
                        var result = JArray.Parse(stringData);
                        address = (string)result[0]["addressNotes"];
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            address = JsonConvert.SerializeObject(address);
            return true;
        }

        /// <summary>
        /// Альтернативный запрос по API, используемому сайтом публичной кадастровой карты росреестра
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendRequestTyped()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var baseAddress = $"https://pkk.rosreestr.ru/api/features/{req_type}?text={cad_num}";

                    client.BaseAddress = new Uri(baseAddress);

                    var response = await client.GetAsync(baseAddress);//, newData);

                    if (response.IsSuccessStatusCode)
                    {
                        var stringData = await response.Content.ReadAsStringAsync();
                        var result = JObject.Parse(stringData);
                        address = (string)result["features"][0]["attrs"]["address"];
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            address = JsonConvert.SerializeObject(address);
            return true;
        }
    }
}