﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asana
{
    /// <summary>
    /// Class represents an Incoming Webhook connection in Slack
    /// </summary>
    public class Webhook
    {
        public string url { get; set; }
        public string channel { get; set; }
        public string username { get; set; }
        public string icon_emoji { get; set; }
        public string icon_url { get; set; }
        public string channel_id { get; set; }
        public string configuration_url { get; set; }

        /// <summary>
        /// Create a webhook for use with Slack.
        /// </summary>
        /// <param name="Url">The URL to send data to</param>
        /// <param name="Channel">The Slack channel to post to, defaults to #general</param>
        /// <param name="User">The name of the user to post as</param>
        /// <param name="EmojiIcon">The emoji to use as icon. Uses Slack syntax, like :ghost: or :rocket:</param>
        /// <param name="UrlIcon">An URL to an image to use as the icon.</param>
        public Webhook(string Url, string Channel = "general", string User = "DynaSlack", string EmojiIcon = null, string UrlIcon = null)
        {
            // check URL
            if (!String.IsNullOrEmpty(Url) && Web.Helpers.checkURI(new Uri(Url))) this.url = Url;
            else throw new Exception("Invalid webhook URL");

            // check channel & username
            if (!String.IsNullOrEmpty(Channel)) this.channel = Channel;
            if (!this.channel.StartsWith("#")) this.channel = "#" + this.channel;
            if (User != null) username = User;

            // check emoji & icon URL
            if (!String.IsNullOrEmpty(EmojiIcon)) this.icon_emoji = EmojiIcon;
            if (!String.IsNullOrEmpty(UrlIcon) && Web.Helpers.checkURI(new Uri(UrlIcon))) this.icon_url = UrlIcon;
        }

        /// <summary>
        /// Post a message using webhooks
        /// </summary>
        /// <param name="webhook">Specify a webhook object to use. If nothing is supplied, it defaults to the Client's webhook.</param>
        /// <param name="name">The text to send</param>
        /// <returns>The message JSON payload</returns>
        public string CreateTask(string name, string description)
        {
            // perform checks before encoding objects
            if (String.IsNullOrEmpty(name)) throw new Exception("Name of task cannot be empty!");

            // build payload
            Asana.Task payload = new Asana.Task(name, description);
                /* set additional properties here
            payload.Workspace = "";
            payload.Project = "";
            */

            // encode payload as JSON and POST it
            string jsonPayload = payload.ToJSON();
            string response = Web.Request.POST(this.url, jsonPayload);

            // validate response
            if (String.IsNullOrEmpty(response)) throw new Exception("Asana servers returned an error.");

            // POST the message and return the server response
            return response;

            // TODO : deserialise server response to the right Asana class instead of returning string
        }

    }
}
