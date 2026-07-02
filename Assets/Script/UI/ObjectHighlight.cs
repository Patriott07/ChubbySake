using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    private Renderer _renderer;
    private Color _originalColor;
    [SerializeField] private Color highlightColor = Color.white;
    private Outline _outline;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        
        // Menambahkan komponen Outline secara dinamis jika belum ada
        _outline = gameObject.GetComponent<Outline>();
        if (_outline == null)
        {
            _outline = gameObject.AddComponent<Outline>();
        }
        _outline.OutlineMode = Outline.Mode.OutlineAll;
        _outline.OutlineColor = Color.white;
        _outline.OutlineWidth = 5f;
        _outline.enabled = false;
    }

    void OnMouseEnter()
    {
        if (_outline != null) _outline.enabled = true;
    }

    void OnMouseExit()
    {
        if (_outline != null) _outline.enabled = false;
    }
}
