using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class BezierCurveRenderer : MonoBehaviour
{
    public enum CurveType { Quadratic, Cubic }

    [Header("Type de courbe")]
    public CurveType curveType = CurveType.Quadratic;

    [Header("Points de contrôle (enfantés ou assignés)")]
    public Transform P0;
    public Transform P1;
    public Transform P2;
    public Transform P3; // utilisé seulement en cubique

    [Header("Affichage")]
    [Range(2, 256)] public int segments = 64; // nombre de points échantillonnés
    public bool updateEveryFrame = true;      // met à jour en temps réel (Play & Edit)
    public bool drawGizmos = true;
    public float gizmoSize = 0.1f;
    public Color gizmoColorPoints = Color.yellow;
    public Color gizmoColorPolygon = new Color(1f, 0.6f, 0f, 0.9f);

    LineRenderer lr;

    void Reset()
    {
        // Auto-setup de base
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.widthMultiplier = 0.05f;

        // Génération automatique des points si absents
        EnsureControlPoints(true);
        UpdateCurve();
    }

    void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
        EnsureControlPoints(false);
        UpdateCurve();
    }

    void OnValidate()
    {
        // Dès qu’un param change dans l’inspecteur
        EnsureControlPoints(false);
        UpdateCurve();
    }

    void Update()
    {
        if (updateEveryFrame) UpdateCurve();
    }

    public void UpdateCurve()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!AllPointsReady()) return;

        int count = Mathf.Max(2, segments);
        lr.positionCount = count;

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0f : i / (float)(count - 1);
            Vector3 pt = (curveType == CurveType.Quadratic)
                ? EvaluateQuadratic(P0.position, P1.position, P2.position, t)
                : EvaluateCubic(P0.position, P1.position, P2.position, P3.position, t);

            lr.SetPosition(i, pt);
        }
    }

    bool AllPointsReady()
    {
        if (curveType == CurveType.Quadratic)
            return P0 && P1 && P2;
        else
            return P0 && P1 && P2 && P3;
    }

    void EnsureControlPoints(bool createIfMissing)
    {
        // Crée des enfants par défaut si non assignés
        if (!P0 || !P1 || !P2 || (curveType == CurveType.Cubic && !P3))
        {
            if (!createIfMissing) return;

            if (!P0) P0 = CreateChild("P0", new Vector3(-2, 0, -2));
            if (!P1) P1 = CreateChild("P1", new Vector3(0, 0, 2));
            if (!P2) P2 = CreateChild("P2", new Vector3(2, 0, -2));
            if (curveType == CurveType.Cubic && !P3)
                P3 = CreateChild("P3", new Vector3(4, 0, -2));
        }
    }

    Transform CreateChild(string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    // --------- Evaluations (De Casteljau) ---------

    public static Vector3 EvaluateQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }

    public static Vector3 EvaluateCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }

    // --------- Gizmos pour repères visuels ---------

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = gizmoColorPoints;
        if (P0) Gizmos.DrawSphere(P0.position, gizmoSize);
        if (P1) Gizmos.DrawSphere(P1.position, gizmoSize);
        if (P2) Gizmos.DrawSphere(P2.position, gizmoSize);
        if (curveType == CurveType.Cubic && P3) Gizmos.DrawSphere(P3.position, gizmoSize);

        // Polygone de contrôle
        Gizmos.color = gizmoColorPolygon;
        if (P0 && P1) Gizmos.DrawLine(P0.position, P1.position);
        if (P1 && P2) Gizmos.DrawLine(P1.position, P2.position);
        if (curveType == CurveType.Cubic && P2 && P3) Gizmos.DrawLine(P2.position, P3.position);
    }
}
