namespace IncidentManagementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModelChange : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.IncidentReports", name: "Crew_Id", newName: "InvestigationCrew_Id");
            RenameIndex(table: "dbo.IncidentReports", name: "IX_Crew_Id", newName: "IX_InvestigationCrew_Id");
            AddColumn("dbo.Crews", "Working", c => c.Boolean(nullable: false));
            AddColumn("dbo.IncidentReports", "MaxValue", c => c.Double(nullable: false));
            AddColumn("dbo.IncidentReports", "CurrentValue", c => c.Double(nullable: false));
            AddColumn("dbo.IncidentReports", "RepairCrew_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.IncidentReports", "RepairCrew_Id");
            AddForeignKey("dbo.IncidentReports", "RepairCrew_Id", "dbo.Crews", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.IncidentReports", "RepairCrew_Id", "dbo.Crews");
            DropIndex("dbo.IncidentReports", new[] { "RepairCrew_Id" });
            DropColumn("dbo.IncidentReports", "RepairCrew_Id");
            DropColumn("dbo.IncidentReports", "CurrentValue");
            DropColumn("dbo.IncidentReports", "MaxValue");
            DropColumn("dbo.Crews", "Working");
            RenameIndex(table: "dbo.IncidentReports", name: "IX_InvestigationCrew_Id", newName: "IX_Crew_Id");
            RenameColumn(table: "dbo.IncidentReports", name: "InvestigationCrew_Id", newName: "Crew_Id");
        }
    }
}
