using FeedbackExtractor.ContentUnderstanding.Implementations;
using FeedbackExtractor.ContentUnderstanding.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedbackExtractor.ContentUnderstanding.Extensions
{
    /// <summary>
    /// Extension methods for registering Content Understanding services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Content Understanding client to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddContentUnderstandingClient(this IServiceCollection services)
        {
            services.AddSingleton<IBlobStorageClient, BlobStorageClient>();
            services.AddSingleton<IContentUnderstandingClient, ContentUnderstandingClient>();
            return services;
        }
    }
}
