using System.Collections.Generic;
using System.Linq;
using CnP.Core;
using CnP.Domain.Unit;
using CnP.Flow;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 棋盘渲染 + RTS 式手动布阵交互（§7.2.3 定稿，仅出牌阶段）：
    /// 左键拖空白=框选 / 左键点单位=单选 / 双击=全选同类 / Shift+左键=增减选 / Ctrl+A=全选
    /// 右键单击=整组保持阵型移动 / Shift+右键拖=线列落子（水平距离=半长，垂直=行位，松开确认，Esc 取消）
    /// 战斗阶段全部布阵操作禁用（状态机直接不响应）。
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        static readonly Dictionary<UnitRole, Color> RoleColors = new Dictionary<UnitRole, Color>
        {
            { UnitRole.战士, new Color(0.76f, 0.34f, 0.30f) },
            { UnitRole.坦克, new Color(0.30f, 0.47f, 0.77f) },
            { UnitRole.射手, new Color(0.33f, 0.66f, 0.38f) },
            { UnitRole.辅助, new Color(0.79f, 0.64f, 0.31f) },
        };
        static readonly Color EnemyTint = new Color(0.55f, 0.32f, 0.55f);

        enum DragState { None, BoxDrag, LineDrag }

        Transform _root;
        readonly Dictionary<int, GameObject> _views = new Dictionary<int, GameObject>();
        readonly Dictionary<int, SpriteRenderer> _bodies = new Dictionary<int, SpriteRenderer>();
        readonly Dictionary<int, Transform> _hpFills = new Dictionary<int, Transform>();

        DragState _state = DragState.None;
        Vector2 _boxStartWorld;      // 框选起点（世界坐标）
        Vector2 _pointerWorld;       // 当前指针（世界坐标）
        bool _boxAdditive;           // 框选是否追加（Shift）
        Vector2 _lineAnchor;         // 线列中心锚点
        float _lineHalf;             // 线列拖拽半长
        float _lastClickTime = -1f;  // 双击检测
        int _lastClickUnitId;

        // 线列预览幽灵池
        readonly List<SpriteRenderer> _ghosts = new List<SpriteRenderer>();
        Transform _ghostRoot;

        Font _font;
        GUIStyle _hintStyle;
        bool _styleBuilt;

        const float BarWidth = 0.7f;
        const float BarY = 0.42f;
        const float DoubleClickWindow = 0.35f;
        const float ClickDragThreshold = 6f; // 屏幕像素：小于此视为单击而非拖拽

        void Awake()
        {
            _root = new GameObject("Units").transform;
            _ghostRoot = new GameObject("LinePreview").transform;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FlowEvents.BoardChanged += Rebuild;
        }

        void OnDestroy()
        {
            FlowEvents.BoardChanged -= Rebuild;
        }

        void Update()
        {
            var flow = RoundFlowController.Instance;
            if (flow == null) return;
            SyncViews(flow);
            if (flow.Phase == Phase.Play) HandleInput(flow);
            else if (_state != DragState.None) CancelDrag();
        }

        // ── 视图同步 ─────────────────────────────────────

        void Rebuild()
        {
            foreach (var go in _views.Values) Destroy(go);
            _views.Clear();
            _bodies.Clear();
            _hpFills.Clear();

            var flow = RoundFlowController.Instance;
            if (flow == null) return;
            foreach (var unit in flow.Board.Units)
                CreateView(unit);
        }

        void CreateView(UnitInstance unit)
        {
            var root = new GameObject("Unit_" + unit.Id + "_" + unit.Stats.Name);
            root.transform.SetParent(_root, false);
            root.transform.position = new Vector3(unit.Position.x, unit.Position.y, 0f);

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeAssets.WhiteSquare;
            sr.sortingOrder = 10;
            sr.color = ColorFor(unit);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.font = _font;
            tm.text = unit.Stats.Name;
            tm.fontSize = 30;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.88f, 0.88f, 0.92f);
            var mr = labelGo.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;
            mr.sortingOrder = 12;

            var hpBg = MakeBar(root.transform, "HpBg", new Color(0.08f, 0.08f, 0.1f), 11);
            hpBg.localPosition = new Vector3(0f, BarY, 0f);
            var hpFill = MakeBar(root.transform, "HpFill", Color.green, 12);
            _hpFills[unit.Id] = hpFill.transform;

            _views[unit.Id] = root;
            _bodies[unit.Id] = sr;
        }

        static Transform MakeBar(Transform parent, string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(BarWidth, 0.085f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeAssets.WhiteSquare;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        void SyncViews(RoundFlowController flow)
        {
            foreach (var unit in flow.Board.Units)
            {
                if (!_views.TryGetValue(unit.Id, out var go)) continue;
                go.transform.position = new Vector3(unit.Position.x, unit.Position.y, 0f);
                if (_bodies.TryGetValue(unit.Id, out var sr))
                {
                    bool selected = unit.Side == Side.Player && flow.Board.SelectedIds.Contains(unit.Id);
                    var baseColor = ColorFor(unit);
                    sr.color = selected ? Color.Lerp(baseColor, Color.white, 0.45f) : baseColor;
                    go.transform.GetChild(0).localScale = selected
                        ? new Vector3(0.86f, 0.86f, 1f)
                        : new Vector3(0.72f, 0.72f, 1f);
                }
                if (_hpFills.TryGetValue(unit.Id, out var fill))
                {
                    float f = unit.MaxHp > 0f ? Mathf.Clamp01(unit.Hp / unit.MaxHp) : 0f;
                    fill.localScale = new Vector3(BarWidth * f, 0.085f, 1f);
                    fill.localPosition = new Vector3(-BarWidth * 0.5f + BarWidth * f * 0.5f, BarY, 0f);
                    fill.GetComponent<SpriteRenderer>().color = Color.Lerp(
                        new Color(0.8f, 0.28f, 0.22f), new Color(0.32f, 0.78f, 0.38f), f);
                }
            }
        }

        // ── 输入状态机（仅出牌阶段）──────────────────────

        void HandleInput(RoundFlowController flow)
        {
            var cam = Camera.main;
            if (cam == null) return;
            _pointerWorld = ScreenToWorld(Input.mousePosition, cam);

            // Ctrl/Cmd+A 全选
            if (Input.GetKeyDown(KeyCode.A) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)))
            {
                flow.Board.SelectAllPlayers();
                return;
            }

            // Esc 取消线列拖拽
            if (Input.GetKeyDown(KeyCode.Escape) && _state == DragState.LineDrag)
            {
                CancelDrag();
                return;
            }

            // ── 左键：单选/双击同类/框选 ──
            if (Input.GetMouseButtonDown(0))
            {
                var hit = HitPlayerUnit(flow);
                if (hit != null)
                {
                    bool doubleClick = Time.time - _lastClickTime < DoubleClickWindow && _lastClickUnitId == hit.Id;
                    if (doubleClick) flow.Board.SelectAllOfType(hit.Stats.Name);
                    else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) flow.Board.ToggleSelect(hit);
                    else flow.Board.SelectSingle(hit);
                    _lastClickTime = Time.time;
                    _lastClickUnitId = hit.Id;
                }
                else
                {
                    _state = DragState.BoxDrag;
                    _boxStartWorld = _pointerWorld;
                    _boxAdditive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                }
            }
            if (Input.GetMouseButtonUp(0) && _state == DragState.BoxDrag)
            {
                float dragPx = (Input.mousePosition - WorldToScreen(_boxStartWorld, cam)).magnitude;
                if (dragPx < ClickDragThreshold)
                {
                    if (!_boxAdditive) flow.Board.ClearSelection(); // 点空白取消选择
                }
                else
                {
                    flow.Board.SelectInRect(_boxStartWorld, _pointerWorld, _boxAdditive);
                }
                _state = DragState.None;
            }

            // ── 右键：整组移动 / Shift 拖线列 ──
            if (Input.GetMouseButtonDown(1) && flow.Board.SelectedIds.Count > 0)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    _state = DragState.LineDrag;
                    _lineAnchor = _pointerWorld;
                    _lineHalf = 0f;
                }
                // 无 Shift：等抬起时按（无拖动）整组移动——拖动到哪就移到哪
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (_state == DragState.LineDrag)
                {
                    flow.Board.PlaceSelectedInLine(_lineAnchor, _lineHalf);
                    _state = DragState.None;
                    UpdateGhostPreview(flow, 0);
                }
                else if (flow.Board.SelectedIds.Count > 0)
                {
                    flow.Board.MoveSelectedGroupTo(_pointerWorld);
                }
            }

            // 线列拖拽中：水平距离决定半长
            if (_state == DragState.LineDrag)
                _lineHalf = Mathf.Abs(_pointerWorld.x - _lineAnchor.x);

            UpdateGhostPreview(flow, _state == DragState.LineDrag
                ? flow.Board.GetSelected().Count : 0);
        }

        void CancelDrag()
        {
            _state = DragState.None;
            UpdateGhostPreview(RoundFlowController.Instance, 0);
        }

        UnitInstance HitPlayerUnit(RoundFlowController flow)
        {
            foreach (var u in flow.Board.Units)
            {
                if (u.Side != Side.Player || !u.Alive) continue;
                if (Vector2.Distance(_pointerWorld, u.Position) <= 0.45f) return u;
            }
            return null;
        }

        // ── 渲染辅助：框选矩形 + 线列预览 ───────────────

        void OnGUI()
        {
            if (_state == DragState.BoxDrag)
            {
                var cam = Camera.main;
                if (cam == null) return;
                var a = WorldToScreen(_boxStartWorld, cam);
                var b = Input.mousePosition;
                var rect = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y),
                                            Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
                var guiRect = new Rect(rect.x, Screen.height - rect.yMax, rect.width, rect.height);
                var tex = RuntimeAssets.WhiteSquare.texture;
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.15f);
                GUI.DrawTexture(guiRect, tex);
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                GUI.DrawTexture(new Rect(guiRect.x, guiRect.y, guiRect.width, 1f), tex);
                GUI.DrawTexture(new Rect(guiRect.x, guiRect.yMax - 1f, guiRect.width, 1f), tex);
                GUI.DrawTexture(new Rect(guiRect.x, guiRect.y, 1f, guiRect.height), tex);
                GUI.DrawTexture(new Rect(guiRect.xMax - 1f, guiRect.y, 1f, guiRect.height), tex);
                GUI.color = prev;
            }

            if (_state == DragState.LineDrag)
            {
                if (!_styleBuilt)
                {
                    _hintStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 14,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(0.95f, 0.9f, 0.65f) },
                    };
                    _styleBuilt = true;
                }
                var flow = RoundFlowController.Instance;
                int n = flow != null ? flow.Board.GetSelected().Count : 0;
                GUI.Label(new Rect(0f, 96f, Screen.width, 22f),
                    "线列落子：" + n + " 个单位（水平拖=长度，垂直=行位；松开确认，Esc 取消）", _hintStyle);
            }
        }

        /// <summary>线列预览幽灵块（半透明），与确认落位共用同一算法保证所见即所得</summary>
        void UpdateGhostPreview(RoundFlowController flow, int count)
        {
            if (flow == null || count == 0)
            {
                foreach (var g in _ghosts) g.gameObject.SetActive(false);
                return;
            }

            var arranged = FormationPlanner.ArrangeLine(
                _lineAnchor, count, _lineHalf,
                BoardGeometry.PlayerZoneWidth(), BoardGeometry.PlayerZoneHeight());

            while (_ghosts.Count < arranged.Length)
            {
                var go = new GameObject("Ghost");
                go.transform.SetParent(_ghostRoot, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeAssets.WhiteSquare;
                sr.sortingOrder = 9;
                sr.color = new Color(1f, 1f, 1f, 0.35f);
                _ghosts.Add(sr);
            }
            for (int i = 0; i < _ghosts.Count; i++)
            {
                bool used = i < arranged.Length;
                _ghosts[i].gameObject.SetActive(used);
                if (used)
                {
                    var p = BoardGeometry.ClampToPlayerZone(arranged[i]);
                    _ghosts[i].transform.position = new Vector3(p.x, p.y, 0f);
                }
            }
        }

        // ── 坐标换算 ─────────────────────────────────────

        static Vector3 ScreenToWorld(Vector3 screen, Camera cam)
        {
            return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
        }

        static Vector3 WorldToScreen(Vector2 world, Camera cam)
        {
            var s = cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));
            return new Vector3(s.x, s.y, 0f);
        }

        Color ColorFor(UnitInstance unit)
        {
            var baseColor = RoleColors[unit.Stats.Role];
            return unit.Side == Side.Enemy
                ? Color.Lerp(baseColor, EnemyTint, 0.65f)
                : baseColor;
        }
    }
}
