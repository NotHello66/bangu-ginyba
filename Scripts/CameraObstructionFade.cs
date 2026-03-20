using Godot;
using System.Collections.Generic;

public partial class CameraObstructionFade : Node3D
{
    [Export] public float fadeAlpha = 0.3f;
    [Export] CharacterBody3D player;

    private Dictionary<MeshInstance3D, Material> fadedObjects = new();

    public override void _Process(double delta)
    {
        if (player == null) return;

        Vector3 from = GlobalTransform.Origin;
        Vector3 to = player.GlobalTransform.Origin;

        var space = GetWorld3D().DirectSpaceState;
        RestoreObjects();

        var exceptions = new Godot.Collections.Array<Rid> { player.GetRid() };

        for (int i = 0; i < 10; i++) //max 10 object see thru
        {
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollideWithAreas = false;
            query.Exclude = exceptions;

            var result = space.IntersectRay(query);
            if (result.Count == 0) break;

            var collider = result["collider"].As<GodotObject>();
            GD.Print("Collider: " + collider.GetType());
            if (collider is CollisionObject3D collision)
            {
                GD.Print("Hit: " + collision.Name);
                if (collision == null) break;
                if (collision.IsInGroup("SeeThru"))
                {
                    var meshes = FindAllMeshes(collision);
                    foreach (var mesh in meshes)
                    {
                        GD.Print("Mesh: " + mesh.Name);
                        FadeObject(mesh);
                    }

                    exceptions.Add(collision.GetRid());
                }
            }
        }
    }

    private List<MeshInstance3D> FindAllMeshes(Node node)
    {
        var meshes = new List<MeshInstance3D>();
        SearchAllRecursive(node, meshes);
        return meshes;
    }

    private void SearchAllRecursive(Node node, List<MeshInstance3D> meshes)
    {
        if (node is MeshInstance3D mesh)
            meshes.Add(mesh);

        foreach (Node child in node.GetChildren())
            SearchAllRecursive(child, meshes);
    }

    private void FadeObject(MeshInstance3D mesh)
    {
        if (fadedObjects.ContainsKey(mesh)) return;

        var original = mesh.GetActiveMaterial(0);
        if (original == null)
        {
            var fallback = new StandardMaterial3D();
            fallback.AlbedoColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            fallback.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            fallback.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            Color fc = fallback.AlbedoColor;
            fc.A = fadeAlpha;
            fallback.AlbedoColor = fc;
            mesh.SetSurfaceOverrideMaterial(0, fallback);
            fadedObjects[mesh] = null;
            return;
        }

        fadedObjects[mesh] = original;

        var fadeMat = original.Duplicate() as BaseMaterial3D;
        if (fadeMat == null) return;

        fadeMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        fadeMat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        Color color = fadeMat.AlbedoColor;
        color.A = fadeAlpha;
        fadeMat.AlbedoColor = color;

        mesh.SetSurfaceOverrideMaterial(0, fadeMat);
    }

    private void RestoreObjects()
    {
        foreach (var pair in fadedObjects)
        {
            if (IsInstanceValid(pair.Key))
                pair.Key.SetSurfaceOverrideMaterial(0, pair.Value);
        }
        fadedObjects.Clear();
    }
}
