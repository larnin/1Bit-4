using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class SaveWorld
{
    public static void EditorReset(bool resetGrid = true)
    {
        if (EditorGridBehaviour.instance == null)
            return;

        if (BuildingList.instance != null)
            BuildingList.instance.Clear();

        if (EntityList.instance != null)
            EntityList.instance.Clear();

        if (ProjectileList.instance != null)
            ProjectileList.instance.Clear();

        if (QuestElementList.instance != null)
            QuestElementList.instance.Clear();

        if (resetGrid)
            EditorGridBehaviour.instance.CreateInitialGrid();
    }

    public static JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if(grid.grid != null)
        {
            var gridSave = GridEx.Save(grid.grid);
            obj.AddElement("grid", gridSave);
        }

        if(BuildingList.instance != null)
        {
            var buildings = BuildingList.instance.Save();
            obj.AddElement("buildings", buildings);
        }

        if(QuestElementList.instance != null)
        {
            var elems = QuestElementList.instance.Save();
            obj.AddElement("questElements", elems);
        }

        if(EntityList.instance != null)
        {
            var entities = EntityList.instance.Save();
            obj.AddElement("entities", entities);
        }

        if(ProjectileList.instance != null)
        {
            var projectiles = ProjectileList.instance.Save();
            obj.AddElement("projectiles", projectiles);
        }

        return obj;
    }

    public static void Load(JsonObject obj)
    {
        var gridJson = obj.GetElement("grid");
        if(gridJson != null && gridJson.IsJsonObject())
        {
            var grid = GridEx.Load(gridJson.JsonObject());

            if (grid != null)
            {
                if (GridBehaviour.instance != null)
                    GridBehaviour.instance.SetGrid(grid);
                Event<SetGridEvent>.Broadcast(new SetGridEvent(grid));
            }
        }

        if (BuildingList.instance != null)
        {
            var buildingsJson = obj.GetElement("buildings");
            if (buildingsJson != null && buildingsJson.IsJsonObject())
                BuildingList.instance.Load(buildingsJson.JsonObject());
        }

        if(QuestElementList.instance != null)
        {
            var elemsJson = obj.GetElement("questElements");
            if (elemsJson != null && elemsJson.IsJsonObject())
                QuestElementList.instance.Load(elemsJson.JsonObject());
        }

        if(EntityList.instance != null)
        {
            var entitiesJson = obj.GetElement("entities");
            if (entitiesJson != null && entitiesJson.IsJsonObject())
                EntityList.instance.Load(entitiesJson.JsonObject());
        }

        if(ProjectileList.instance != null)
        {
            var projectilesJson = obj.GetElement("projectiles");
            if (projectilesJson != null && projectilesJson.IsJsonObject())
                ProjectileList.instance.Load(projectilesJson.JsonObject());
        }
    }
}
