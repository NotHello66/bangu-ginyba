using Godot;
using System;

public partial class PlayerControllerMouse : Node
{
    [Export] Camera3D camera;
    public override void _Ready()
    {
        
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
    /// <summary>
    /// Sauna ray ir grazina koordinates su pirmu collisionu ir colliding objekta
    /// </summary>
    /// <param name="hitPosition"></param>
    /// <param name="collider"></param>
    /// <returns>true jei kazka hitino, false jei ne</returns>
    public bool RayCastFromMouse(out Vector3 hitPosition, out GodotObject collider)
    {
        Vector3 rayOrigin;
        Vector3 rayTarget;
        hitPosition = Vector3.Zero;
        collider = null;
        var mousePosition = GetViewport().GetMousePosition();
        rayOrigin = camera.ProjectRayOrigin(mousePosition);
        rayTarget = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000;

        var spaceState = GetTree().Root.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayTarget);
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;
        var intersect = spaceState.IntersectRay(query);
        if (intersect.Count == 0)
        {
            return false;
        }
        hitPosition = (Vector3)intersect["position"];
        collider = (GodotObject)intersect["collider"];
        return true;

    }
}
