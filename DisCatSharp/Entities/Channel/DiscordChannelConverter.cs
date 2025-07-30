/*using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DisCatSharp.Enums;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Polymorphic converter for Discord channel types.
    /// </summary>
    public class DiscordChannelConverter : JsonConverter<BaseDiscordChannel>
    {
        public override BaseDiscordChannel ReadJson(JsonReader reader, Type objectType, BaseDiscordChannel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var type = (ChannelType)jo["type"].Value<int>();
            BaseDiscordChannel channel;
            switch (type)
            {
                case ChannelType.Private:
                    channel = new DiscordDmChannel();
                    break;
                case ChannelType.Group:
                    channel = new DiscordGroupDmChannel();
                    break;
                case ChannelType.Text:
                    channel = new DiscordTextChannel();
                    break;
                case ChannelType.News:
                    channel = new DiscordAnnouncementChannel();
                    break;
                case ChannelType.Forum:
                    channel = new DiscordForumChannel();
                    break;
                case ChannelType.Media:
                    channel = new DiscordMediaChannel();
                    break;
                case ChannelType.PublicThread:
                case ChannelType.PrivateThread:
                case ChannelType.NewsThread:
                    channel = new DiscordThreadChannel();
                    break;
                case ChannelType.Voice:
                    channel = new DiscordVoiceChannel();
                    break;
                case ChannelType.Stage:
                    channel = new DiscordStageChannel();
                    break;
                case ChannelType.Category:
                    channel = new DiscordCategoryChannel();
                    break;
                default:
                    channel = new DiscordChannel(); // fallback
                    break;
            }
            serializer.Populate(jo.CreateReader(), channel);
            return channel;
        }

        public override void WriteJson(JsonWriter writer, BaseDiscordChannel value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
*/
