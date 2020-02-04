namespace Compact.Models
{
    public class LinkModel
    {
        /// <summary>
        /// The URL which cmpct.io links too, as provided by the user.
        /// </summary>
        public string Target { get; set; }

        public string Title { get; set; }
    }
}