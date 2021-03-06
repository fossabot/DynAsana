﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Asana
{
    /// <summary>
    /// Individual error message from Asana API.
    /// </summary>
    internal class AsanaError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("phrase")]
        public string Phrase { get; set; }
    }

    /// <summary>
    /// Response from the Asana API detailing the errors that occured.
    /// </summary>
    internal class AsanaErrorResponse
    {
        [JsonProperty("errors")]
        internal List<AsanaError> Errors { get; set; }
    }
}
