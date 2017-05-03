﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asana
{
    /// <summary>
    /// Represents any request to the Asana API. Inherits from the RestSharp RestRequest class.
    /// </summary>
    public class AsanaRequest
    {
        internal TimeSpan runtime { get; private set; }
        internal RestRequest restRequest { get; private set; }
        public Asana.AsanaResponse errorResponse;

        public AsanaRequest(Asana.Client client, Method method, string resource = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.restRequest = new RestRequest();
            this.restRequest.AddHeader("Authorization", client.Token);
            this.restRequest.AddHeader("Asana-Fast-Api", "true");
            this.restRequest.AddHeader("cache-control", "no-cache");
            this.restRequest.AddHeader("content-type", "application/x-www-form-urlencoded");
            this.restRequest.AddHeader("DynAsana-token", Assembly.GetExecutingAssembly().GetType().GUID.ToString());

            this.restRequest.Resource = resource;
            this.restRequest.Method = method;
        }

        /// <summary>
        /// Executes an Asana Request
        /// </summary>
        /// <typeparam name="T">The Asana object type to deserialize as.</typeparam>
        /// <param name="request">The Asana Request to execute.</param>
        /// <returns>Response from Asana API deserialized as the supplied type.</returns>
        internal T Execute<T>(Asana.Client client) where T : new()
        {
            //try
            //{
                var startTime = DateTime.Now;
                var response = client.restClient.Execute(this.restRequest);
                this.runtime = DateTime.Now - startTime;
                Console.WriteLine("Request took " + runtime.TotalMilliseconds.ToString() + "ms");
                return Deserialize<T>(response, client.JsonTokenOverride);
            //}
            //catch (Exception e)
            //{
            //    throw new ApplicationException(e.Message);
            //}
        }

        /// <summary>
        /// Deserializes a response from the Asana API to the supplied object type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="response">The response from Asana API to deserialize.</param>
        /// <param name="JsonToken">Optional : specify a JSON token as the root element to deserialise from.
        /// Default for Asana is : "$.data"</param>
        /// <returns>The supplied response from Asana API, deserialized as the supplied type.</returns>
        internal T Deserialize<T>(IRestResponse response, string JsonToken = null) where T : new()
        {
            /// We first need to check there is something to serialise.
            /// If Asana didn't return a success code, we parse the error message instead of deserialising.
            /// Successful web response codes are in the 100 and 200 series. Larger numbers indicate warnings or errors.
            if (Asana.Helpers.ServerReturnedSuccessfulResponse(response)==false)
            {
                this.errorResponse = JsonConvert.DeserializeObject<AsanaResponse>(response.Content);

                /// The Asana API is capable of returning multiple errors
                /// We need to parse each one and record its error message.
                string errorMessage = "";
                foreach (var error in errorResponse.Errors)
                {
                    if (error != null && !String.IsNullOrEmpty(error.Message))
                        errorMessage += "Error :" + error.Message + Environment.NewLine;
                }

                throw new InvalidOperationException(errorMessage);
            }

            /// Because Asana wraps all responses in a "data{}" object in JSON,
            /// we then need to check if the client has a Json token override.
            /// Specifying this at client level will allow simultaneous usage of different Asana API versions should this change.
            string taskData = "";
            if (String.IsNullOrEmpty(JsonToken) == false)
            {
                JObject o = JObject.Parse(response.Content);
                taskData = o.SelectToken(JsonToken).ToString();
            }
            else taskData = response.Content;

            /// We don't want the deserialisation to break if some properties are empty.
            /// So we need to specify the behaviour when such values are encountered.
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            return JsonConvert.DeserializeObject<T>(taskData, settings);
        }


    }


}
