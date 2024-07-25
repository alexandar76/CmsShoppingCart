namespace CmsShoppingCart.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Orders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblOrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.tblProducts", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.tblUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.UserId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.tblOrders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId)
                //.ForeignKey("dbo.tblUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblOrderDetails", "UserId", "dbo.tblUsers");
            DropForeignKey("dbo.tblOrderDetails", "ProductId", "dbo.tblProducts");
            DropForeignKey("dbo.tblOrderDetails", "OrderId", "dbo.tblOrders");
            DropForeignKey("dbo.tblOrders", "UserId", "dbo.tblUsers");
            DropIndex("dbo.tblOrders", new[] { "UserId" });
            DropIndex("dbo.tblOrderDetails", new[] { "ProductId" });
            DropIndex("dbo.tblOrderDetails", new[] { "UserId" });
            DropIndex("dbo.tblOrderDetails", new[] { "OrderId" });
            DropTable("dbo.tblOrders");
            DropTable("dbo.tblOrderDetails");
        }
    }
}
