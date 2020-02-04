using System;
using System.Collections.Generic;

namespace Compact.Models
{
    /// <summary>
    /// A unique collection of one or more links to other websites
    /// </summary>
    public class RouteModel
    {
        /// <summary>
        /// The unique identifier associated with this route
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A list of one or more links that this route can direct a user too
        /// </summary>
        public IEnumerable<LinkModel> Links { get; set; }

        /// <summary>
        /// A password which must be provided to download this route
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Record the date that the route was processed, so we can skip it in future
        /// </summary>
        public DateTime? ProcessDate { get; set; }
    }
}