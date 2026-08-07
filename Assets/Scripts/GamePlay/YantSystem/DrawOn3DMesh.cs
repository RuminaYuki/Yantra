using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// คลาส DrawOn3DMesh จัดการการวาดเส้นบนโมเดล 3 มิติ
/// เส้นจะถูกเก็บเป็น Local Space ของกระดาษเพื่อให้เคลื่อนที่ตามกระดาษได้ถูกต้อง
/// </summary>
public class DrawOn3DMesh : MonoBehaviour
{
    [SerializeField] private LineRenderer _linePrefab;
    [SerializeField] private YantraInputObserverSO _inputObserver;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _drawableLayer;
    private Vector3 _lastLocalPoint;
    private bool _hasLastPoint = false;

    [SerializeField]
    private float _minPointDistance = 0.002f; // ระยะขั้นต่ำระหว่าง Point บนกระดาษ

    [Header("Drawing Setup")]
    [Tooltip("วัตถุหลัก (กระดาษ) ที่จะให้เส้นทั้งหมดเข้าไปอยู่เป็นลูก")]
    [SerializeField] private Transform _paperParent;

    [Header("Drawing Offset")]
    [Tooltip("ระยะห่างระหว่างเส้นปากกากับพื้นผิววัตถุ")]
    [SerializeField] private float _surfaceOffset = 0.05f;

    private LineRenderer _currentLine;
    private bool _isDrawing = false;
    private Vector2 _lastMousePos;  

    // เก็บรายการเส้นทั้งหมดที่วาดใน Session นี้
    [SerializeField]private List<LineRenderer> _allStrokes = new List<LineRenderer>();

    private void OnEnable()
    {
        if (_inputObserver)
            _inputObserver.OnLeftClickChannel += OnLeftClickInput;

        // บังคับให้เมาส์แสดงผลและปลดล็อก
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        if (_inputObserver)
            _inputObserver.OnLeftClickChannel -= OnLeftClickInput;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_isDrawing || _currentLine == null)
            return;

        AddPointToLine(Mouse.current.position.ReadValue());
    }

    private void OnLeftClickInput(Vector2 clickPos, InputAction.CallbackContext context)
    {
        _isDrawing = context.ReadValueAsButton();

        if (_isDrawing)
        {
            // เริ่ม stroke แรกของการวาดรอบนี้ → เล่นเสียงพากย์ "ขณะวาด" ครั้งเดียว
            /*if (_allStrokes.Count == 0)
                PlayerVoice.Publish(PlayerVoice.WhileDrawing);*/

            // สร้างเส้นใหม่ทุกครั้งที่คลิกใหม่เพื่อไม่ให้เชื่อมกับเส้นเดิม
            CreateNewLine(clickPos);
        }
        else
        {
            _currentLine = null;
            _hasLastPoint = false;
        }
    }

    private void CreateNewLine(Vector2 startPos)
    {
        _currentLine = Instantiate(_linePrefab);

        if (_paperParent != null)
        {
            _currentLine.transform.SetParent(_paperParent, false);
        }

        _currentLine.useWorldSpace = false;
        _currentLine.positionCount = 0;

        _allStrokes.Add(_currentLine);

        _lastMousePos = startPos;

        // รีเซ็ตข้อมูลของเส้นใหม่
        _hasLastPoint = false;

        AddPointToLine(startPos);
    }

    private void AddPointToLine(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _drawableLayer))
            return;

        Vector3 worldPoint = hitInfo.point + hitInfo.normal * _surfaceOffset;
        Vector3 localPoint = _paperParent.InverseTransformPoint(worldPoint);

        // เช็คระยะจาก Point ก่อนหน้าใน Local Space
        if (_hasLastPoint)
        {
            if (Vector3.Distance(_lastLocalPoint, localPoint) < _minPointDistance)
                return;
        }

        _currentLine.positionCount++;
        _currentLine.SetPosition(_currentLine.positionCount - 1, localPoint);

        _lastLocalPoint = localPoint;
        _hasLastPoint = true;

        _lastMousePos = screenPos;
    }

    /// <summary>
    /// ฟังก์ชันดึงพิกัดทั้งหมดเพื่อนำไปวัดความเหมือน (เปรียบเทียบ)
    /// ต้องแปลงกลับเป็น World Space ก่อนนำไปเทียบค่า
    /// </summary>
    public List<Vector3> GetAllDrawnPointsInWorldSpace()
    {
        List<Vector3> allPoints = new List<Vector3>();

        foreach (var stroke in _allStrokes)
        {
            if (stroke == null) continue;

            for (int i = 0; i < stroke.positionCount; i++)
            {
                // ดึงตำแหน่งจาก LineRenderer (ซึ่งเป็น Local) แล้วแปลงกลับเป็น World
                Vector3 localPoint = stroke.GetPosition(i);
                Vector3 worldPoint = _paperParent.TransformPoint(localPoint);
                allPoints.Add(worldPoint);
            }
        }
        Debug.Log(allPoints.Count);
        return allPoints;
    }

    public void ClearDrawing()
    {
        foreach (var stroke in _allStrokes)
        {
            if (stroke != null) Destroy(stroke.gameObject);
        }
        _allStrokes.Clear();
        _currentLine = null;
    }

    //API
    public Transform PaperParent => _paperParent;
}