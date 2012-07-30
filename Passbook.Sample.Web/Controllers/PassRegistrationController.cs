﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Passbook.Sample.Web.Services;
using Passbook.Generator;
using System.Text;
using System.IO;

namespace Passbook.Sample.Web.Controllers
{
    public class PassRegistrationController : ApiController
    {
        //https://webServiceURL/version/devices/deviceLibraryIdentifier/registrations/passTypeIdentifier/serialNumber
        public async Task<HttpResponseMessage> Post(string version, string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, HttpRequestMessage message)
        {
            string authorizationToken = message.Headers.Authorization.Parameter;

            string jsonBody = await message.Content.ReadAsStringAsync();

            JObject json = JObject.Parse(jsonBody);
            string pushToken = json["pushToken"].Value<string>();

            PassRegistrationResult result = PassRegistrationResult.SuccessfullyRegistered;

            //this.handler.RegisterPass(version, deviceLibraryIdentifier, passTypeIdentifier, serialNumber, pushToken, authorizationToken, out result);

            HttpStatusCode resultCode = HttpStatusCode.InternalServerError;

            switch (result)
            {
                case PassRegistrationResult.SuccessfullyRegistered:
                    resultCode = HttpStatusCode.Created;
                    break;
                case PassRegistrationResult.UnAuthorized:
                    resultCode = HttpStatusCode.Unauthorized;
                    break;
                case PassRegistrationResult.AlreadyRegistered:
                    resultCode = HttpStatusCode.OK;
                    break;
            }

            return new HttpResponseMessage(resultCode);
        }

        //https://webServiceURL/version/devices/deviceLibraryIdentifier/registrations/passTypeIdentifier?passesUpdatedSince=tag
        public HttpResponseMessage Get(string version, string deviceLibraryIdentifier, string passTypeIdentifier, HttpRequestMessage request)
        {
            //List<string> updatedSerialNumbers = new List<string>();
            //updatedSerialNumbers.Add("121212111");

            //Dictionary<string, string> outputDictionary = new Dictionary<string, string>();
            //outputDictionary.Add("lastUpdated", "21/07/2012");
            //outputDictionary.Add("serialNumbers", JsonConvert.SerializeObject(updatedSerialNumbers));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            //string json = JsonConvert.SerializeObject(outputDictionary);


            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("lastUpdated");
                writer.WriteValue("21/07/2012");
                writer.WritePropertyName("serialNumbers");
                writer.WriteStartArray();
                writer.WriteValue("121212111");
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");

            return response;
        }

        //https://webServiceURL/version/passes/passTypeIdentifier/serialNumber
        public HttpResponseMessage GetPass(string version, string passTypeIdentifier, string serialNumber)
        {
            StoreCardGeneratorRequest request = new StoreCardGeneratorRequest();
            request.Identifier = "pass.tomasmcguinness.coupons";
            request.CertThumbnail = ConfigurationManager.AppSettings["PassBookCertificateThumbnail"];
            request.CertLocation = System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser;
            request.FormatVersion = 1;
            request.SerialNumber = "121212111";
            request.Description = "My first pass";
            request.OrganizationName = "Tomas McGuinness";
            request.TeamIdentifier = "Team America";
            request.LogoText = "My Pass";
            request.BackgroundColor = "#000000";
            request.ForegroundColor = "#FFFFFF";

            request.BackgroundFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/background.png");
            request.BackgroundRetinaFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/background@2x.png");

            request.IconFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/icon.png");
            request.IconRetinaFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/icon@2x.png");

            request.LogoFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/logo.png");
            request.LogoRetinaFile = HttpContext.Current.Server.MapPath(@"~/Icons/Starbucks/logo@2x.png");

            // Specific information
            //
            request.Balance = 121.12;
            request.OwnersName = "Tomas McGuinness";
            request.Title = "Starbucks";
            request.AddBarCode("01927847623423234234", BarcodeType.PKBarcodeFormatPDF417, "UTF-8", "01927847623423234234");

            request.AuthenticationToken = "vxwxd7J8AlNNFPS8k0a0FfUFtq0ewzFdc";
            request.WebServiceUrl = "http://192.168.1.3:81/api/";

            PassGenerator generator = new PassGenerator();
            Pass generatedPass = generator.Generate(request);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<Byte[]>(generatedPass.GetPackage(), new BinaryFormatter());
            return response;
        }
    }
}