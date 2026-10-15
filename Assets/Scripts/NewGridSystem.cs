using UnityEngine;
using UnityEngine.InputSystem;

public class NewGridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    public int gridWidth = 20;
    public int gridHeight = 20;
    public Vector2Int spawnCell = new Vector2Int(0, 0);
    public Vector2Int goalCell = new Vector2Int(19, 19);
    public LayerMask groundMask = ~0;   // set to your ground layer in the Inspector

    private GameObject ghostObject;
    private GridData grid;
    private FlowField flowField;
    private Vector2Int hoveredCell;
    private bool hasValidHover;

    private void Start()
    {
        grid = new GridData(gridWidth, gridHeight);
        flowField = new FlowField(grid);
        flowField.Rebuild(goalCell);
        CreateGhostObject();
    }

    private void Update()
    {
        UpdateGhostPosition();

        if (hasValidHover && Mouse.current.leftButton.wasPressedThisFrame)
            PlaceObject();
    }

    Vector2Int WorldToCell(Vector3 p) =>
        new Vector2Int(Mathf.RoundToInt(p.x / gridSize), Mathf.RoundToInt(p.z / gridSize));

    Vector3 CellToWorld(Vector2Int c) =>
        new Vector3(c.x * gridSize, 0f, c.y * gridSize);

    void UpdateGhostPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        hasValidHover = false;
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask)) return;

        Vector2Int cell = WorldToCell(hit.point);
        if (!grid.InBounds(cell)) return;

        hoveredCell = cell;
        hasValidHover = true;
        ghostObject.transform.position = CellToWorld(cell);

        bool blocked = grid.IsBlocked(cell) || cell == spawnCell || cell == goalCell;
        SetGhostColor(blocked ? Color.red : new Color(1f, 1f, 1f, 0.5f));
    }

    void PlaceObject()
    {
        Vector2Int cell = hoveredCell;
        if (grid.IsBlocked(cell) || cell == spawnCell || cell == goalCell) return;

        // Try it, and undo if it seals the path
        grid.SetCell(cell, CellType.Tower);
        flowField.Rebuild(goalCell);

        if (!flowField.IsReachable(spawnCell))
        {
            grid.SetCell(cell, CellType.Empty);
            flowField.Rebuild(goalCell);
            return;
        }

        Instantiate(objectToPlace, CellToWorld(cell), Quaternion.identity);
    }

        void CreateGhostObject()
    {
        if (ghostObject == null)
        {
            ghostObject = Instantiate(objectToPlace);
            ghostObject.GetComponent<Collider>().enabled = false; // Disable collider for the ghost object
        }

        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            Color color = material.color;
            color.a = 0.5f; // Set alpha to 50% for transparency
            material.color = color;

            material.SetFloat("_Mode", 2); // Set rendering mode to transparent
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

        void SetGhostColor(Color color)
    {
        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();

        foreach(Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.color = color;

        }
    }




    //

    public bool showGizmos = true;

    private void OnDrawGizmos()
    {
        if (!showGizmos || grid == null || flowField == null) return;

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                var cell = new Vector2Int(x, y);
                Vector3 center = CellToWorld(cell) + Vector3.up * 0.05f;

                // Cell squares
                if (cell == spawnCell)
                    Gizmos.color = Color.green;
                else if (cell == goalCell)
                    Gizmos.color = Color.blue;
                else if (grid.IsBlocked(cell))
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                else
                    Gizmos.color = new Color(1f, 1f, 1f, 0.15f);

                Gizmos.DrawWireCube(center, new Vector3(gridSize, 0.01f, gridSize));

                // Flow arrows
                if (grid.IsBlocked(cell) || !flowField.IsReachable(cell)) continue;

                Vector2Int dir = flowField.GetDirection(cell);
                if (dir == Vector2Int.zero) continue;

                Vector3 d = new Vector3(dir.x, 0f, dir.y) * gridSize * 0.35f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(center - d, center + d);
                Gizmos.DrawSphere(center + d, gridSize * 0.07f);   // dot marks the arrow head

    #if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * 0.1f,
                    flowField.GetDistance(cell).ToString());
    #endif
            }
        }
    }
}