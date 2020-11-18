using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultBot
{
	public static class Events
	{
		/// <summary>
		/// Default Event for Encoding tasks
		/// </summary>
		public static EventId EncodeStart { get; } = new EventId(201, "ENCODER_STR");
		public static EventId EncodeEnd { get; } = new EventId(202, "ENCODER_END");
		/// <summary>
		/// Default Event for Queue Related Events
		/// </summary
		public static EventId Queue { get; } = new EventId(210, "QUEUE");
		public static EventId QueueAdd { get; } = new EventId(211, "QUEUE_ADD");
		/// <summary>
		/// Default Event for Anime Related Events
		/// </summary>
		public static EventId AnimePublished { get; } = new EventId(202, "NEW_ANIME");
	}
}
