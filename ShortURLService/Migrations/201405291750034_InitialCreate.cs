namespace ShortURLService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.URLs",
                c => new
                    {
                        UrlId = c.Int(nullable: false, identity: true),
                        LongUrl = c.String(nullable: false),
                        ShortUrl = c.String(nullable: false),
                        Hits = c.Int(nullable: false),
                        UserId = c.Int(),
                    })
                .PrimaryKey(t => t.UrlId);

            //CreateIndex("dbo.URLs", new string[] { "LongUrl", "ShortUrl" },
            //    true, "IX_URLs_LongUrl");

            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        LastLoginDate = c.DateTime(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.URLs");
            //DropIndex("dbo.URLs", "IX_URLs_LongUrl");
        }
    }
}
