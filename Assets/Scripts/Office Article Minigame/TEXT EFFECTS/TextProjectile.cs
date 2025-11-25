using System;
using UnityEngine;
using TMPro;

public class TextProjectile : MonoBehaviour
{
    [HideInInspector] public RectTransform RectTransform;
    [HideInInspector] public TextMeshProUGUI Text;

    private Vector2 _velocity;
    private Vector2 _gravity;
    private RectTransform _canvasRect;
    private Action<TextProjectile> _onDespawn;
    private float _lifeTime;
    private float _age;
    private bool _isActive;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        Text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Inicializa el proyectil con todos sus parámetros
    /// </summary>
    public void Init(
        Vector2 startPos,
        Vector2 initialVelocity,
        Vector2 gravity,
        float lifeTime,
        RectTransform canvasRect,
        Action<TextProjectile> onDespawn)
    {
        _velocity = initialVelocity;
        _gravity = gravity;
        _canvasRect = canvasRect;
        _lifeTime = lifeTime;
        _onDespawn = onDespawn;
        _age = 0f;
        _isActive = true;

        RectTransform.anchoredPosition = startPos;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_isActive) return;

        float dt = Time.deltaTime;
        _age += dt;

        // Física simple en espacio UI: v = v + g * t, p = p + v * t
        _velocity += _gravity * dt;
        RectTransform.anchoredPosition += _velocity * dt;

        if (_age >= _lifeTime || IsOutsideBounds())
        {
            Despawn();
        }
    }

    private bool IsOutsideBounds()
    {
        if (_canvasRect == null) return false;

        Vector2 pos = RectTransform.anchoredPosition;
        Rect r = _canvasRect.rect;
        float margin = 100f; // un poquito más para que salgan de pantalla

        return pos.x < r.xMin - margin ||
               pos.x > r.xMax + margin ||
               pos.y < r.yMin - margin ||
               pos.y > r.yMax + margin;
    }

    private void Despawn()
    {
        if (!_isActive) return;
        _isActive = false;
        _onDespawn?.Invoke(this);
    }
}
