namespace ShortURLService.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ShortURLService.DAL.UrlContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ShortURLService.DAL.UrlContext";
        }

        protected override void Seed(ShortURLService.DAL.UrlContext context)
        {
            context.Urls.AddOrUpdate(u => u.LongUrl,
                new Models.URL { LongUrl = "firstUrl.com", ShortUrl = "a", GeneratedDate=DateTime.Now }

                );

        }
    }
}
