namespace ECOLAB.IOT.SiteManagement.Middlewares
{
    using ECOLAB.IOT.SiteManagement.Middlewares.TraceMiddleware;

    /// <summary>
    /// MiddlewareExtension
    /// </summary>
    public static class MiddlewareExtension
    {
        /// <summary>
        /// Use Exception Middleware
        /// </summary>
        /// <param name="app">app</param>
        public static void UseExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware(typeof(ExceptionMiddleware));
        }
    }
}
